using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public class RandomFailureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Random _random = new();

        public RandomFailureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var minute = DateTime.UtcNow.Minute;
            var errorProbability = (minute % 2 == 0) ? 0.7 : 0.3;

            if (_random.NextDouble() < errorProbability)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Simulated random failure");
                return;
            }

            await _next(context);
        }
    }
}