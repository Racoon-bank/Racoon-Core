using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.Metrics
{
    public class LogSenderService : BackgroundService
    {
        private readonly LogBuffer _buffer;
        private readonly IHttpClientFactory _factory;

        public LogSenderService(LogBuffer buffer, IHttpClientFactory factory)
        {
            _buffer = buffer;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2000, stoppingToken);

                var logs = _buffer.Flush(50);

                if (!logs.Any())
                    continue;

                try
                {
                    var client = _factory.CreateClient("monitoring");
                    await client.PostAsJsonAsync("/api/logs/batch", logs, stoppingToken);
                }
                catch
                {
                }
            }
        }
    }
}