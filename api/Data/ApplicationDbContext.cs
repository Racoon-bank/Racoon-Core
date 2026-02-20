using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions) 
        : base()
        {
            
        }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankAccountOperation> BankAccountOperations { get; set; }
    }
}