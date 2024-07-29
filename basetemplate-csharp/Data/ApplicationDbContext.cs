using System;
using basetemplate_csharp.Models;
using Microsoft.EntityFrameworkCore;

namespace basetemplate_csharp.Data
{
	public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AutomatedTellerMachine> AutomatedTellerMachines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("presential_service_channels");
            base.OnModelCreating(modelBuilder);
        }
    }
}

