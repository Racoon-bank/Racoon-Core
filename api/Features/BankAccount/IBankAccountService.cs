using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.BankAccount.Dto;
using api.Models;

namespace api.Features.BankAccount
{
    public interface IBankAccountService
    {
        Task<List<BankAccountDto>> GetAllByIdAsync(Guid userId);
        Task<List<BankAccountDto>> GetAllAsync();
        Task<BankAccountDto> AddAsync(Guid userId, CreateBankAccountDto dto);
        Task DeleteAsync(Guid bankAccountId, string? userId);
        Task<List<BankAccountOperationDto>> GetHistoryAsync(Guid bankAccountId);
        Task DepositMoneyAsync(Guid id, MoneyOperationDto operationDto, string? userId);
        Task WithdrawMoneyAsync(Guid id, MoneyOperationDto operationDto, string? userId);
        Task ApplyCredit(Guid id, MoneyOperationDto operationDto);
        Task PayCredit(Guid id, MoneyOperationDto operationDto);
        Task<BankAccountDto> ChangeVisibility(Guid id, Guid userId);
    }
}