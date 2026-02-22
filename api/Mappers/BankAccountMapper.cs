using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.BankAccount.Dto;
using api.Models;

namespace api.Mappers
{
    public static class BankAccountMapper
    {
        public static BankAccountDto ToBankAccountDto(this BankAccount bankAccount)
        {
            return new BankAccountDto
            {
                Id = bankAccount.Id,
                AccountNumber = bankAccount.AccountNumber,
                Balance = bankAccount.Balance,
                CreatedAt = bankAccount.CreatedAt,
            };
        }

        public static BankAccountOperationDto ToBankAccountOperationDto(
            this BankAccountOperation operation
        )
        {
            return new BankAccountOperationDto
            {
                Id = operation.Id,
                Amount = operation.Amount,
                Type = operation.Type,
                CreatedAt = operation.CreatedAt,
            };
        }
    }
}
