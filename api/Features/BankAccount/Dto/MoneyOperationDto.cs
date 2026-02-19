using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.BankAccount.Dto
{
    public class MoneyOperationDto
    {
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}