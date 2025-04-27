using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Главное меню администратора
        public IActionResult Index()
        {
            return View();
        }

        // Выход администратора
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout([FromServices] SignInManager<ApplicationUser> signInManager)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // Управление пользователями (просмотр)
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // --- Управление ролями пользователя ---
        [HttpGet]
        public async Task<IActionResult> EditUserRoles(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.AllRoles = allRoles;
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRoles(string id, string[] selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            selectedRoles = selectedRoles ?? Array.Empty<string>();

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при добавлении ролей.");
            }

            result = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при удалении старых ролей.");
            }

            // Аудит — логируем смену ролей
            await LogAudit(
                "Изменение ролей",
                $"Пользователю {user.Email} установлены роли: {string.Join(", ", selectedRoles)}"
            );

            // Журнал транзакций — логируем смену ролей
            await LogTransaction(
                "Update",
                "User",
                user.Id,
                $"Изменены роли пользователя {user.Email}. Текущие роли: {string.Join(", ", selectedRoles)}"
            );

            return RedirectToAction(nameof(ManageUsers));
        }

        // --- Удаление пользователя ---
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при удалении пользователя.");
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserRoles = userRoles;
                return View(user);
            }

            // Аудит — логируем удаление пользователя
            await LogAudit(
                "Удаление пользователя",
                $"Удалён пользователь {user.Email} (Id: {user.Id})"
            );

            // Журнал транзакций — логируем удаление пользователя
            await LogTransaction(
                "Delete",
                "User",
                user.Id,
                $"Удалён пользователь {user.Email}"
            );

            return RedirectToAction(nameof(ManageUsers));
        }

        // --- Просмотр аудита ---
        public async Task<IActionResult> Audit()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }

        // --- Просмотр журнала транзакций ---
        public async Task<IActionResult> TransactionLog()
        {
            var logs = await _context.TransactionLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }

        // --- Приватный метод для записи в журнал аудита ---
        private async Task LogAudit(string action, string details)
        {
            var log = new AuditLog
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                UserEmail = User.Identity?.Name,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // --- Приватный метод для записи в журнал транзакций ---
        private async Task LogTransaction(string operation, string entity, string entityId, string details)
        {
            var log = new TransactionLog
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                UserEmail = User.Identity?.Name,
                Operation = operation,
                Entity = entity,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            _context.TransactionLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // --- Заглушки для остальных функций ---
        public IActionResult Backup() => View();
        public IActionResult Restore() => View();
    }
}
