using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using api.Enum;

namespace api.Models
{
    public class BankAccountOperation
    {
        public Guid Id { get; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public OperationType Type { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid BankAccountId { get; set; }
    }
}
