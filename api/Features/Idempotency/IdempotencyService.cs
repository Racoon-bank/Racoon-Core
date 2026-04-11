using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Features.Idempotency
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ApplicationDbContext _context;
        public IdempotencyService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IdempotencyRecord?> GetAsync(string key)
        {
            return await _context.IdempotencyRecords.FirstOrDefaultAsync(x => x.Key == key);
        }

        public async Task SaveAsync(string key, string response, int statusCode)
        {
            var record = new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                Key = key,
                Response = response,
                StatusCode = statusCode,
                CreatedAt = DateTime.UtcNow
            };

            await _context.IdempotencyRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }
    }
}