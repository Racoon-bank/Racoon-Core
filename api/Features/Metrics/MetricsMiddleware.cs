using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using api.Exceptions;

namespace api.Features.Metrics
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly LogBuffer _buffer;
        public MetricsMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, LogBuffer buffer)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _buffer = buffer;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            int statusCode;

            try
            {
                await _next(context);
                statusCode = context.Response.StatusCode;
            }
            catch (Exception ex)
            {
                statusCode = ex switch
                {
                    BankAccountNotFoundException => 404,
                    InsufficientFundsException => 400,
                    FailedToFetchExchangeRateException => 400,
                    _ => 500
                };

                sw.Stop();
                await SendLog(context, sw.ElapsedMilliseconds, statusCode, ex.Message);

                throw;
            }

            sw.Stop();
            Console.WriteLine(context.Items.ContainsKey("IsDuplicate"));
            await SendLog(context, sw.ElapsedMilliseconds, statusCode, null);
        }

        private async Task SendLog(HttpContext context, long duration, int statusCode, string? message)
        {
            var log = new LogDto
            {
                ServiceName = "Core",
                Path = NormalizePath(context.Request.Path),
                Method = context.Request.Method,
                StatusCode = statusCode,
                DurationMs = (int)duration,
                TraceId = Activity.Current?.Id,
                CreatedAt = DateTime.UtcNow,
                Message = message,
                IsDuplicate = context.Items.ContainsKey("IsDuplicate")
            };

            try
            {
                _buffer.Add(log);
            }
            catch { }
        }

        private string NormalizePath(string path)
        {
            return Regex.Replace(path, @"\b[0-9a-fA-F\-]{36}\b", "{id}");
        }
    }
}