using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;

namespace api.Retries
{
    public static class HttpPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromMilliseconds(200 * retryAttempt));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.7,
                    samplingDuration: TimeSpan.FromSeconds(30),
                    minimumThroughput: 10,
                    durationOfBreak: TimeSpan.FromSeconds(15),
                    onBreak: (result, duration) =>
                    {
                        Console.WriteLine("Circuit open");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit closed");
                    });
        }
    }
}