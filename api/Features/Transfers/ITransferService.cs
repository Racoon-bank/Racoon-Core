using api.Features.BankAccount.Dto;

namespace api.Features.Transfers
{
    public interface ITransferService
    {
        Task<BankAccountDto> TransferMoneyAsync(TransferDto dto, string userId);
    }
}
