using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Enum;

namespace api.Broker
{
    public class OperationMessage
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public OperationType Type { get; set; }
    }
}