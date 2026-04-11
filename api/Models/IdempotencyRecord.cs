using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class IdempotencyRecord
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string Response { get; set; } = null!;
        public int StatusCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}