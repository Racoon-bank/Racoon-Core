using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions) { }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankAccountOperation> BankAccountOperations { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BankAccount>().HasIndex(a => a.AccountNumber).IsUnique();
            builder
                .Entity<BankAccount>()
                .HasMany(a => a.BankAccountHistory)
                .WithOne(o => o.BankAccount)
                .HasForeignKey(o => o.BankAccountId);
        }
    }
}
