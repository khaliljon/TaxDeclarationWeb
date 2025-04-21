using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Taxpayer> Taxpayers { get; set; }
        public DbSet<Declaration> Declarations { get; set; }

        // Здесь будут добавлены остальные таблицы позже
    }
}