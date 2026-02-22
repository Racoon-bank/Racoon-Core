using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Enum;
using api.Exceptions;
using api.Features.BankAccount.Dto;
using api.Interfaces;
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

        public async Task<BankAccountDto> AddAsync(Guid userId)
        {
            var bankAccount = new Models.BankAccount(userId);
            await _context.BankAccounts.AddAsync(bankAccount);
            await _context.SaveChangesAsync();
            return bankAccount.ToBankAccountDto();
        }

        public async Task<Models.BankAccount?> DeleteAsync(Guid bankAccountId) //fix later
        {
            var account = await GetByIdAsync(bankAccountId);
            if (account == null)
                throw new BankAccountNotFoundException(bankAccountId);

            _context.BankAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<BankAccountDto> DepositMoneyAsync(Guid id, MoneyOperationDto operationDto)
        {
            var account = await GetByIdAsync(id);
            if (account == null)
                throw new BankAccountNotFoundException(id);

            var updatedAccount = await UpdateAsync(account, account.Balance + operationDto.Amount);

            await AddBankAccountOperation(
                new BankAccountOperation
                {
                    Id = Guid.NewGuid(),
                    Amount = operationDto.Amount,
                    Type = OperationType.Deposit,
                    CreatedAt = DateTime.UtcNow,
                    BankAccountId = updatedAccount.Id,
                    BankAccount = updatedAccount,
                }
            );
            return updatedAccount.ToBankAccountDto();
        }

        public async Task<BankAccountDto?> WithdrawMoneyAsync(
            Guid id,
            MoneyOperationDto operationDto
        )
        {
            var account = await GetByIdAsync(id);
            if (account == null)
                throw new BankAccountNotFoundException(id);

            var newBalance = account.Balance - operationDto.Amount;
            if (newBalance < 0)
                throw new InsufficientFundsException();

            var updatedAccount = await UpdateAsync(account, newBalance);

            await AddBankAccountOperation(
                new BankAccountOperation
                {
                    Id = Guid.NewGuid(),
                    Amount = operationDto.Amount,
                    Type = OperationType.Withdraw,
                    CreatedAt = DateTime.UtcNow,
                    BankAccountId = updatedAccount.Id,
                    BankAccount = updatedAccount,
                }
            );
            return updatedAccount.ToBankAccountDto();
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

        private async Task AddBankAccountOperation(BankAccountOperation bankAccountOperation)
        {
            await _context.BankAccountOperations.AddAsync(bankAccountOperation);
            await _context.SaveChangesAsync();
        }

        private async Task<Models.BankAccount> UpdateAsync(
            Models.BankAccount account,
            decimal newBalance
        )
        {
            account.Balance = newBalance;
            _context.BankAccounts.Update(account);
            await _context.SaveChangesAsync();
            return account;
        }

        private async Task<Models.BankAccount?> GetByIdAsync(Guid bankAccountId)
        {
            return await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == bankAccountId);
        }

    }
}
