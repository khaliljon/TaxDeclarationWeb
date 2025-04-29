using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using DotNetEnv;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка .env файла ---
Env.Load();
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
Console.WriteLine("== CONNECTION_STRING ==");
Console.WriteLine(connectionString);

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: CONNECTION_STRING is not defined!");
}

// --- Подключение к базе данных ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Identity + роли ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- Авторизация по ролям ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTaxpayer", policy =>
        policy.RequireRole("Taxpayer"));

    options.AddPolicy("RequireInspector", policy =>
        policy.RequireRole("Inspector", "ChiefInspector", "Admin"));

    options.AddPolicy("RequireChiefInspector", policy =>
        policy.RequireRole("ChiefInspector"));

    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
});

// --- Json-параметры ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// --- Инициализация ролей и пользователей ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Taxpayer", "Inspector", "ChiefInspector", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
                Console.WriteLine($"Role '{role}' created successfully.");
            else
                Console.WriteLine($"ERROR creating role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    var testUsers = new List<(string Email, string Password, string Role)>
    {
        ("inspector@tax.local", "Inspector123!", "Inspector"),
        ("chief@tax.local", "Chief123!", "ChiefInspector"),
        ("payer@tax.local", "Payer123!", "Taxpayer")
    };

    foreach (var (email, pass, role) in testUsers)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(user, pass);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                Console.WriteLine($"User '{email}' with role '{role}' created.");
            }
            else
            {
                Console.WriteLine($"ERROR creating user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}

// --- Настройка Rotativa (wkhtmltopdf) ---
RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

// --- Middleware ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    Console.WriteLine("Environment: Development");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// --- Роутинг ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

Console.WriteLine("Application is starting...");
app.Run();
