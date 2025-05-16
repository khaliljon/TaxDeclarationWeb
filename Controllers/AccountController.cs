using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TaxDeclarationWeb.Models;
using Microsoft.AspNetCore.Authorization;
using TaxDeclarationWeb.Data;

namespace TaxDeclarationWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                if (roles.Contains("Admin"))
                    await LogAudit("Вход администратора", $"Вход выполнен: {user.Email}");

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Admin");
                if (roles.Contains("ChiefInspector"))
                    return RedirectToAction("Index", "ChiefInspector");
                if (roles.Contains("Inspector"))
                    return RedirectToAction("Index", "Inspector");
                if (roles.Contains("Taxpayer"))
                    return RedirectToAction("Index", "Taxpayer");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    await LogAudit("Выход администратора", $"Выход выполнен: {user.Email}");
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccessDenied()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);
            
            if (roles.Contains("Admin"))
                await LogAudit("AccessDenied", $"Администратор {user.Email} попытался получить доступ к закрытой зоне: {Request.Path}");
            else if (roles.Contains("ChiefInspector"))
                await LogAudit("AccessDenied", $"Главный инспектор {user.Email} попытался получить доступ к закрытой зоне: {Request.Path}");

            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Admin");
            if (roles.Contains("ChiefInspector"))
                return RedirectToAction("Index", "ChiefInspector");
            if (roles.Contains("Inspector"))
                return RedirectToAction("Index", "Inspector");
            if (roles.Contains("Taxpayer"))
                return RedirectToAction("Index", "Taxpayer");

            return RedirectToAction("Index", "Home");
        }
        
        private async Task LogAudit(string action, string details)
        {
            var log = new AuditLog
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                UserEmail = User.Identity?.Name ?? "Аноним",
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        
    }
}
