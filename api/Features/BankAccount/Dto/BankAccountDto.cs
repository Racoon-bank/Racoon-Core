using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.BankAccount.Dto
{
    public class BankAccountDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = default!;
        public decimal Balance { get; set; }
        public Enum.Currency Currency { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
