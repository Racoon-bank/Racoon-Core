using System.ComponentModel.DataAnnotations;

namespace api.Features.BankAccount.Dto
{
    public class CreateBankAccountDto
    {
        [Required]
        public Enum.Currency Currency { get; set; }
    }
}
