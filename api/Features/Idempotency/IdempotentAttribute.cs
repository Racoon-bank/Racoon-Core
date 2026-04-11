using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Features.Idempotency
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IdempotentAttribute : Attribute
    {
    }
}