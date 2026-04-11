using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api.Features.Idempotency
{
    public class IdempotencyFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAttribute = context.MethodInfo
                .GetCustomAttributes(typeof(IdempotentAttribute), false)
                .Any();

            if (!hasAttribute)
                return;

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Idempotency key for safe retries",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}