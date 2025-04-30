using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;
using DotNetEnv;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Загрузка .env ---
Env.Load();
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: CONNECTION_STRING is not defined!");
}
else
{
    Console.WriteLine("== CONNECTION_STRING ==");
    Console.WriteLine(connectionString);
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

// --- Авторизация ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTaxpayer", policy => policy.RequireRole("Taxpayer"));
    options.AddPolicy("RequireInspector", policy => policy.RequireRole("Inspector", "ChiefInspector", "Admin"));
    options.AddPolicy("RequireChiefInspector", policy => policy.RequireRole("ChiefInspector"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

// --- Json настройки ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.WriteIndented = true;
    });

// --- Локализация: ru-RU для чисел с запятыми ---
var culture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var app = builder.Build();

// --- Инициализация ролей и инспекторов ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    string[] roles = { "Taxpayer", "Inspector", "ChiefInspector", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
                Console.WriteLine($"✅ Роль '{role}' создана.");
            else
                Console.WriteLine($"❌ Ошибка создания роли '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    // --- Инспектора (email + пароль) ---
    var inspectors = new List<(int code, string email, string password)>
    {
        (1, "alekseev@tax.local", "Alekseev123!"),
        (2, "kuznetsova@tax.local", "Kuznetsova123!"),
        (3, "ivanov@tax.local", "Ivanov123!"),
        (4, "sidorova@tax.local", "Sidorova123!"),
        (5, "petrova@tax.local", "Petrova123!")
    };

    foreach (var (code, email, password) in inspectors)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Inspector");

                var inspector = await context.Inspectors.FirstOrDefaultAsync(i => i.Code == code);
                if (inspector != null)
                {
                    inspector.UserId = user.Id;
                    context.Inspectors.Update(inspector);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🔗 Инспектор '{inspector.FullName}' привязан к аккаунту '{email}'");
                }
                else
                {
                    Console.WriteLine($"⚠️ Инспектор с кодом {code} не найден.");
                }
            }
            else
            {
                Console.WriteLine($"❌ Ошибка создания пользователя '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"⏩ Пользователь '{email}' уже существует.");
        }
    }
}

// --- Настройка Rotativa ---
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

Console.WriteLine("🚀 Application is starting...");
app.Run();
