using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using DotNetEnv;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Загрузка .env
Env.Load();
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

// Подключение к базе
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity + роли
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Авторизация по ролям (корректное разграничение!)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTaxpayer", policy =>
        policy.RequireRole("Taxpayer"));

    options.AddPolicy("RequireInspector", policy =>
        policy.RequireRole("Inspector", "ChiefInspector", "Admin"));

    options.AddPolicy("RequireChiefInspector", policy =>
        policy.RequireRole("ChiefInspector")); // Только главный инспектор!

    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
});

// --- ВАЖНО: добавлено IgnoreCycles ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Создание ролей
    string[] roles = { "Taxpayer", "Inspector", "ChiefInspector", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Создание администратора
    var adminEmail = "admin@tax.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };

        var result = await userManager.CreateAsync(user, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    // Тестовые пользователи
    var testUsers = new List<(string Email, string Password, string Role)>
    {
        ("inspector@tax.local", "Inspector123!", "Inspector"),
        ("chief@tax.local", "Chief123!", "ChiefInspector"),
        ("payer@tax.local", "Payer123!", "Taxpayer")
    };

    foreach (var (email, password, role) in testUsers)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
