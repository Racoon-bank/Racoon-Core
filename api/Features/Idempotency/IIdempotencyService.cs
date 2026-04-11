using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Features.Idempotency
{
    public interface IIdempotencyService
    {
        Task<IdempotencyRecord?> GetAsync(string key);
        Task SaveAsync(string key, string response, int statusCode);
    }
}