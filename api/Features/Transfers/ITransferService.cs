using api.Features.BankAccount.Dto;

namespace api.Features.Transfers
{
    public interface ITransferService
    {
        Task TransferMoneyAsync(TransferDto dto, string userId);
    }
}
