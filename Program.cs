using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using DotNetEnv;

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

// Авторизация по ролям
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTaxpayer", policy =>
        policy.RequireRole("Taxpayer"));

    options.AddPolicy("RequireInspector", policy =>
        policy.RequireRole("Inspector", "ChiefInspector", "Admin"));

    options.AddPolicy("RequireChiefInspector", policy =>
        policy.RequireRole("ChiefInspector", "Admin"));

    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ❗️ Временно можно ОТКЛЮЧИТЬ миграции, если они вызывают ошибку
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Если база уже создана — эту строку можно закомментировать
    // var context = services.GetRequiredService<ApplicationDbContext>();
    // context.Database.Migrate();

    // Создание ролей
    string[] roles = { "Taxpayer", "Inspector", "ChiefInspector", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Создание администратора (при первом запуске)
    var adminEmail = "admin@tax.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Role = "Admin"
        };

        var result = await userManager.CreateAsync(user, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
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
