using api.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class BankAccount
    {
        private static readonly Random _random = new();
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public Currency Currency { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BankAccountOperation> BankAccountHistory { get; set; } =
            new List<BankAccountOperation>();

        public BankAccount(Guid userId, Currency currency)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            AccountNumber = GenerateAccountNumber();
            Balance = 0m;
            Currency = currency;
            CreatedAt = DateTime.UtcNow;
            IsHidden = false;
        }

        private static string GenerateAccountNumber()
        {
            return $"{DateTime.UtcNow:yyyyMMdd}"
                + string.Concat(Enumerable.Range(0, 12).Select(_ => _random.Next(0, 10)));
        }
    }
}
