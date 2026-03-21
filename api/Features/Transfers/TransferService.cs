using api.Broker;
using api.Data;
using api.Enum;
using api.Exceptions;
using api.Features.BankAccount.Dto;
using api.Features.Currency;
using api.Mappers;
using api.Models;
using api.Websocket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Features.Transfers
{
    public class TransferService : ITransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly IMessagePublisher _publisher;

        public TransferService(ApplicationDbContext context, ICurrencyService currencyService, IMessagePublisher publisher)
        {
            _context = context;
            _currencyService = currencyService;
            _publisher = publisher;
        }
        public async Task TransferMoneyAsync(TransferDto dto, string userId)
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

            await _publisher.PublishAsync(new TransferMessage
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = dto.Amount,
                ConvertedAmount = convertedAmount
            }, "transfers");
        }

        private async Task<Models.BankAccount> GetBankAccountOrThrowAsync(Guid id)
        {
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == id);
            if (bankAccount == null)
            {
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
