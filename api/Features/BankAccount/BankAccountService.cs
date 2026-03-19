using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Enum;
using api.Exceptions;
using api.Features.BankAccount;
using api.Features.BankAccount.Dto;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Features
{
    public class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;

        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BankAccountDto> AddAsync(Guid userId, CreateBankAccountDto dto)
        {
            var bankAccount = new Models.BankAccount(userId, dto.Currency);
            await _context.BankAccounts.AddAsync(bankAccount);
            await _context.SaveChangesAsync();
            return bankAccount.ToBankAccountDto();
        }

        public async Task DeleteAsync(Guid bankAccountId)
        {
            var account = await GetByIdAsync(bankAccountId);
            if (account == null)
                throw new BankAccountNotFoundException(bankAccountId);

            _context.BankAccounts.Remove(account); // all remaining money goes to poor HITs students
            await _context.SaveChangesAsync();
        }

        public async Task<BankAccountDto> DepositMoneyAsync(Guid id, MoneyOperationDto operationDto)
        {
            var account = await ChangeBalanceAsync(id, operationDto.Amount, OperationType.Deposit);
            return account.ToBankAccountDto();
        }

        public async Task<BankAccountDto> WithdrawMoneyAsync(
            Guid id,
            MoneyOperationDto operationDto
        )
        {
            var account = await ChangeBalanceAsync(id, -operationDto.Amount, OperationType.Withdraw);
            return account.ToBankAccountDto();
        }

        public async Task ApplyCredit(Guid id, MoneyOperationDto operationDto)
        {
            var account = await ChangeBalanceAsync(id, operationDto.Amount, OperationType.CreditIssued);
        }

        public async Task PayCredit(Guid id, MoneyOperationDto operationDto)
        {
            var account = await ChangeBalanceAsync(id, -operationDto.Amount, OperationType.CreditPayment);
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
            var account = await GetByIdAsync(id);
            if (account == null)
                throw new BankAccountNotFoundException(id);
            if (account.UserId != userId)
                throw new UnathorizedAccessException();

            account.IsHidden = !account.IsHidden;
            await _context.SaveChangesAsync();
            return account.ToBankAccountDto();
        }

        private async Task AddBankAccountOperation(BankAccountOperation bankAccountOperation)
        {
            await _context.BankAccountOperations.AddAsync(bankAccountOperation);
            await _context.SaveChangesAsync();
        }

        private async Task<Models.BankAccount?> GetByIdAsync(Guid bankAccountId)
        {
            return await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == bankAccountId);
        }

        private async Task<Models.BankAccount> ChangeBalanceAsync(Guid accountId, decimal amount, OperationType operationType)
        {
            var account = await GetByIdAsync(accountId);
            if (account == null)
                throw new BankAccountNotFoundException(accountId);

            var newBalance = account.Balance + amount;

            if (newBalance < 0)
                throw new InsufficientFundsException();

            account.Balance = newBalance;

            _context.BankAccountOperations.Add(new BankAccountOperation
            {
                Id = Guid.NewGuid(),
                Amount = Math.Abs(amount),
                Type = operationType,
                CreatedAt = DateTime.UtcNow,
                BankAccountId = account.Id
            });

            await _context.SaveChangesAsync();

            return account;
        }
    }
}
