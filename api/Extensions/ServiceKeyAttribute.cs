using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Extensions
{
    public class ServiceKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext
                .RequestServices
                .GetRequiredService<IConfiguration>();

            var expectedKey = configuration["ServiceAuth:CreditServiceKey"];

            if (!context.HttpContext.Request.Headers
                .TryGetValue("X-Service-Key", out var providedKey) ||
                providedKey != expectedKey)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}