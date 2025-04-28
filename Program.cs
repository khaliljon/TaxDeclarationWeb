using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using DotNetEnv;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка .env ---
Env.Load();
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
Console.WriteLine("== CONNECTION_STRING ==");
Console.WriteLine(connectionString);

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: CONNECTION_STRING is not defined in the environment variables!");
}

// --- Подключение к базе ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Identity + роли ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- Авторизация по ролям (корректное разграничение!) ---
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

// --- Добавлено IgnoreCycles для Json ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// --- Создание ролей и тестовых пользователей ---
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
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
                Console.WriteLine($"Role '{role}' created successfully.");
            else
                Console.WriteLine($"ERROR: Failed to create role '{role}'. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        else
        {
            Console.WriteLine($"Role '{role}' already exists.");
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
            Console.WriteLine($"Admin user '{adminEmail}' created successfully.");
        }
        else
        {
            Console.WriteLine($"ERROR: Failed to create admin user '{adminEmail}'. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine($"Admin user '{adminEmail}' already exists.");
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
                Console.WriteLine($"Test user '{email}' with role '{role}' created successfully.");
            }
            else
            {
                Console.WriteLine($"ERROR: Failed to create test user '{email}'. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"Test user '{email}' already exists.");
        }
    }
}

// --- Middleware ---
if (!app.Environment.IsDevelopment())
{
    Console.WriteLine("Environment: Production");
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    Console.WriteLine("Environment: Development");
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot (по умолчанию)
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

Console.WriteLine("Application is starting...");
app.Run();