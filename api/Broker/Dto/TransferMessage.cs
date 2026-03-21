using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Broker
{
    public class TransferMessage
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
    }
}