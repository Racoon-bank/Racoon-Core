using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Broker;
using api.Data;
using api.Enum;
using api.Exceptions;
using api.Features.BankAccount;
using api.Features.BankAccount.Dto;
using api.Mappers;
using api.Models;
using api.Websocket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Features
{
    public class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<BankHub> _hub;
        private readonly IMessagePublisher _publisher;

        public BankAccountService(ApplicationDbContext context, IHubContext<BankHub> hub, IMessagePublisher publisher)
        {
            _context = context;
            _hub = hub;
            _publisher = publisher;
        }

        public async Task<BankAccountDto> AddAsync(Guid userId, CreateBankAccountDto dto)
        {
            var bankAccount = new Models.BankAccount(userId, dto.Currency);
            await _context.BankAccounts.AddAsync(bankAccount);
            await _context.SaveChangesAsync();
            return bankAccount.ToBankAccountDto();
        }

        public async Task DeleteAsync(Guid bankAccountId, string? userId)
        {
            var account = await GetByIdOrThrowAsync(bankAccountId);
            if (userId == null || account.UserId != new Guid(userId))
                throw new UnathorizedAccessException();

            _context.BankAccounts.Remove(account); // all remaining money goes to poor HITs students
            await _context.SaveChangesAsync();
        }

        public async Task DepositMoneyAsync(Guid id, MoneyOperationDto operationDto, string? userId)
        {
            await CheckUserAccess(id, userId);
            await ChangeBalanceAsync(id, operationDto.Amount, OperationType.Deposit);
        }

        public async Task WithdrawMoneyAsync(Guid id, MoneyOperationDto operationDto, string? userId)
        {
            await CheckUserAccess(id, userId);
            await ChangeBalanceAsync(id, -operationDto.Amount, OperationType.Withdraw);
        }

        public async Task ApplyCredit(Guid id, MoneyOperationDto operationDto)
        {
            await ChangeBalanceAsync(id, operationDto.Amount, OperationType.CreditIssued);
        }

        public async Task PayCredit(Guid id, MoneyOperationDto operationDto)
        {
            await ChangeBalanceAsync(id, -operationDto.Amount, OperationType.CreditPayment);
        }

        public async Task<List<BankAccountDto>> GetAllAsync()
        {
            return await _context.BankAccounts.Select(x => x.ToBankAccountDto()).ToListAsync();
        }

        public async Task<List<BankAccountDto>> GetAllByIdAsync(Guid userId)
        {
            return await _context
                .BankAccounts.Where(x => x.UserId == userId)
                .Select(x => x.ToBankAccountDto())
                .ToListAsync();
        }

        public async Task<List<BankAccountOperationDto>> GetHistoryAsync(Guid bankAccountId)
        {
            return await _context
                .BankAccountOperations.Where(x => x.BankAccountId == bankAccountId)
                .Select(x => x.ToBankAccountOperationDto())
                .ToListAsync();
        }

        public async Task<BankAccountDto> ChangeVisibility(Guid id, Guid userId)
        {
            var account = await GetByIdOrThrowAsync(id);
            if (account.UserId != userId)
                throw new UnathorizedAccessException();

            account.IsHidden = !account.IsHidden;
            await _context.SaveChangesAsync();
            return account.ToBankAccountDto();
        }

        private async Task<Models.BankAccount> GetByIdOrThrowAsync(Guid bankAccountId)
        {
            var account = await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == bankAccountId);
            if (account == null)
                throw new BankAccountNotFoundException(bankAccountId);
            return account;
        }

        private async Task CheckUserAccess(Guid bankAccountId, string? userId)
        {
            var account = await GetByIdOrThrowAsync(bankAccountId);
            if (userId == null || account.UserId != new Guid(userId))
                throw new UnathorizedAccessException();
        }

        private async Task ChangeBalanceAsync(Guid accountId, decimal amount, OperationType operationType)
        {
            var account = await GetByIdOrThrowAsync(accountId);

            var newBalance = account.Balance + amount;
            if (newBalance < 0)
                throw new InsufficientFundsException();

            await _publisher.PublishAsync(new OperationMessage
            {
                AccountId = account.Id,
                Amount = amount,
                Type = operationType
            }, "operations");

            // account.Balance = newBalance;
            // _context.BankAccountOperations.Add(new BankAccountOperation
            // {
            //     Id = Guid.NewGuid(),
            //     Amount = Math.Abs(amount),
            //     Type = operationType,
            //     CreatedAt = DateTime.UtcNow,
            //     BankAccountId = account.Id
            // });

            // await _context.SaveChangesAsync();

            // await _hub.Clients.Group(account.Id.ToString()).SendAsync("OperationCreated", new
            // {
            //     accountId = account.Id
            // });
        }
    }
}
