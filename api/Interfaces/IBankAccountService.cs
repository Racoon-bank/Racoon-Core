using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.BankAccount.Dto;
using api.Models;

namespace api.Interfaces
{
    public interface IBankAccountService
    {
        Task<List<BankAccountDto>> GetAllByIdAsync(Guid userId);
        Task<List<BankAccountDto>> GetAllAsync();
        Task<BankAccountDto> AddAsync(Guid userId);
        Task<BankAccount?> DeleteAsync(Guid bankAccountId);
        Task<List<BankAccountOperationDto>> GetHistoryAsync(Guid bankAccountId);
        Task<BankAccountDto> DepositMoneyAsync(Guid id, MoneyOperationDto operationDto);
        Task<BankAccountDto> WithdrawMoneyAsync(Guid id, MoneyOperationDto operationDto);
        Task ApplyCredit(Guid id, MoneyOperationDto operationDto);
        Task PayCredit(Guid id, MoneyOperationDto operationDto);
    }
}