using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.BankAccount.Enum;

namespace api.Features.BankAccount.Dto
{
    public class BankAccountOperationDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public OperationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}