using api.Data;
using api.Enum;
using api.Exceptions;
using api.Features.BankAccount.Dto;
using api.Features.Currency;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Features.Transfers
{
    public class TransferService : ITransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public TransferService(ApplicationDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }
        public async Task<BankAccountDto> TransferMoneyAsync(TransferDto dto, string userId)
        {
            var fromAccount = await GetBankAccountOrThrowAsync(dto.FromAccountId);
            if (fromAccount.UserId != new Guid(userId))
                throw new UnathorizedAccessException();
            if (fromAccount.Balance - dto.Amount < 0)
                throw new InsufficientFundsException();

            var toAccount = await GetBankAccountByNumberOrThrowAsync(dto.ToAccountNumber);

            decimal convertedAmount = dto.Amount;
            if (fromAccount.Currency != toAccount.Currency)
            {
                var rate = await _currencyService.GetExchangeRateAsync(fromAccount.Currency, toAccount.Currency);
                convertedAmount = Math.Round(dto.Amount * rate, 2);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                fromAccount.Balance -= dto.Amount;
                toAccount.Balance += convertedAmount;

                var now = DateTime.UtcNow;
                var withdrawOperation = new BankAccountOperation
                {
                    Id = Guid.NewGuid(),
                    Amount = dto.Amount,
                    Type = OperationType.TransferOut,
                    CreatedAt = now,
                    BankAccountId = fromAccount.Id
                };
                var depositOperation = new BankAccountOperation
                {
                    Id = Guid.NewGuid(),
                    Amount = convertedAmount,
                    Type = OperationType.TransferIn,
                    CreatedAt = now,
                    BankAccountId = toAccount.Id
                };

                _context.BankAccountOperations.AddRange(withdrawOperation, depositOperation);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            return fromAccount.ToBankAccountDto();
        }

        private async Task<Models.BankAccount> GetBankAccountOrThrowAsync(Guid id)
        {
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == id);
            if (bankAccount == null) {
                throw new BankAccountNotFoundException(id);
            }
            return bankAccount;
        }

        private async Task<Models.BankAccount> GetBankAccountByNumberOrThrowAsync(string number)
        {
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.AccountNumber == number);
            if (bankAccount == null)
            {
                throw new BankAccountNotFoundException(number);
            }
            return bankAccount;
        }
    }
}
