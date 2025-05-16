using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace TaxDeclarationWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Taxpayer> Taxpayers { get; set; }
        public DbSet<Declaration> Declarations { get; set; }
        public DbSet<Inspector> Inspectors { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TransactionLog> TransactionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Taxpayer>().ToTable("Налогоплательщики", t => t.ExcludeFromMigrations());
            builder.Entity<Declaration>().ToTable("Декларации", t => t.ExcludeFromMigrations());
            builder.Entity<Inspector>().ToTable("Инспекторы", t => t.ExcludeFromMigrations());
            builder.Entity<Inspection>().ToTable("Налоговые_инспекции", t => t.ExcludeFromMigrations());
            builder.Entity<Category>().ToTable("Категории_плательщиков", t => t.ExcludeFromMigrations());
            builder.Entity<Country>().ToTable("Страны", t => t.ExcludeFromMigrations());
            builder.Entity<Nationality>().ToTable("Национальности", t => t.ExcludeFromMigrations());

            builder.Entity<Declaration>().Property(d => d.Income).HasPrecision(18, 2);
            builder.Entity<Declaration>().Property(d => d.Expenses).HasPrecision(18, 2);
            builder.Entity<Declaration>().Property(d => d.NonTaxableExpenses).HasPrecision(18, 2);
            builder.Entity<Declaration>().Property(d => d.PaidTaxes).HasPrecision(18, 2);

        }
    }
}
