using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.Metrics
{
    public class LogBuffer
    {
        private readonly ConcurrentQueue<LogDto> _queue = new();

        public void Add(LogDto log)
        {
            _queue.Enqueue(log);
        }

        public List<LogDto> Flush(int maxBatchSize)
        {
            var result = new List<LogDto>();

            while (result.Count < maxBatchSize && _queue.TryDequeue(out var log))
            {
                result.Add(log);
            }

            return result;
        }
    }
}