using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.Metrics
{
    public class LogDto
    {
        public string ServiceName { get; set; } = null!;
        public string? Path { get; set; }
        public string? Method { get; set; }
        public int StatusCode { get; set; }
        public int DurationMs { get; set; }
        public string? TraceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Message { get; set; }
        public bool IsDuplicate { get; set; }
    }
}