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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Привязка к таблицам с русскими именами
            builder.Entity<Taxpayer>().ToTable("Налогоплательщики");
            builder.Entity<Declaration>().ToTable("Декларации");
            builder.Entity<Inspector>().ToTable("Инспекторы");
            builder.Entity<Inspection>().ToTable("Налоговые_инспекции");
            builder.Entity<Category>().ToTable("Категории_плательщиков");
            builder.Entity<Country>().ToTable("Страны");
            builder.Entity<Nationality>().ToTable("Национальности");

            // Decimal поля с точностью
            builder.Entity<Declaration>().Property(d => d.Income).HasPrecision(18, 2);
            builder.Entity<Declaration>().Property(d => d.Expenses).HasPrecision(18, 2);
            builder.Entity<Declaration>().Property(d => d.NonTaxableExpenses).HasPrecision(18, 2);
            builder.Entity<Declaration>().Property(d => d.PaidTaxes).HasPrecision(18, 2);

            // ВНИМАНИЕ: внешняя связь InspectorId → инспектор отключена
            // В ApplicationUser используется [NotMapped] для свойства Inspector
        }
    }
}
