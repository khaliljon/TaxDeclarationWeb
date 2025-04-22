using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Models;
namespace TaxDeclarationWeb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Только чтение, не включаются в миграции
        builder.Entity<Taxpayer>().ToView("Налогоплательщики");
        builder.Entity<Declaration>().ToView("Декларации");
        builder.Entity<Inspector>().ToView("Инспекторы");
        builder.Entity<Inspection>().ToView("Налоговые_инспекции");
        builder.Entity<Category>().ToView("Категории_плательщиков");
        builder.Entity<Country>().ToView("Страны");
        builder.Entity<Nationality>().ToView("Национальности");
    }
}