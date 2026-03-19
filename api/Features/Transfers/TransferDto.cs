using System.ComponentModel.DataAnnotations;

namespace api.Features.Transfers
{
    public class TransferDto
    {
        public Guid FromAccountId { get; set; }

        [StringLength(20, ErrorMessage = "Bank account number is 20 characters long")]
        public string ToAccountNumber { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
