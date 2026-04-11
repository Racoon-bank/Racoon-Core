using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.Idempotency
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;

        public IdempotencyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IIdempotencyService service)
        {
            var endpoint = context.GetEndpoint();

            Console.WriteLine(endpoint?.DisplayName);

            var hasAttribute = endpoint?.Metadata.GetMetadata<IdempotentAttribute>() != null;

            Console.WriteLine($"Has Idempotent: {hasAttribute}");
            var key = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(key))
            {
                await _next(context);
                return;
            }

            var existing = await service.GetAsync(key);
            if (existing != null)
            {
                context.Response.StatusCode = existing.StatusCode;
                await context.Response.WriteAsync(existing.Response);
                return;
            }

            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(memoryStream).ReadToEndAsync();

            if (context.Response.StatusCode < 500)
            {
                await service.SaveAsync(key, responseText, context.Response.StatusCode);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBody);
        }
    }
}