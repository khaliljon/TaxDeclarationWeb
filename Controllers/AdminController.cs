using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        // Главная панель администратора
        public IActionResult Index() => View();

        // Выход администратора с аудитом
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout([FromServices] SignInManager<ApplicationUser> signInManager)
        {
            await LogAudit("Выход администратора", $"Выход выполнен: {User.Identity?.Name}");
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // --- Управление пользователями ---
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserRoles(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            selectedRoles = selectedRoles ?? Array.Empty<string>();

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
            if (!result.Succeeded)
                ModelState.AddModelError("", "Ошибка при добавлении ролей.");

            result = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
            if (!result.Succeeded)
                ModelState.AddModelError("", "Ошибка при удалении старых ролей.");

            await LogAudit("Изменение ролей", $"Пользователю {user.Email} установлены роли: {string.Join(", ", selectedRoles)}");
            await LogTransaction("Update", "User", user.Id, $"Изменены роли пользователя {user.Email}. Текущие роли: {string.Join(", ", selectedRoles)}");

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = userRoles;
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при удалении пользователя.");
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserRoles = userRoles;
                return View(user);
            }

            await LogAudit("Удаление пользователя", $"Удалён пользователь {user.Email} (Id: {user.Id})");
            await LogTransaction("Delete", "User", user.Id, $"Удалён пользователь {user.Email}");
            return RedirectToAction(nameof(ManageUsers));
        }

        // --- Аудит ---
        public async Task<IActionResult> Audit()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }

        // --- Журнал транзакций ---
        public async Task<IActionResult> TransactionLog()
        {
            var logs = await _context.TransactionLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }

        // --- РЕЗЕРВНОЕ КОПИРОВАНИЕ ---
        [HttpGet]
        public IActionResult Backup()
        {
            ViewBag.Result = TempData["Result"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Backup(string backupPath)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                backupPath = @"C:\Users\khaly\OneDrive\Desktop\TaxDeclaration.bak";

            string dbName = "TaxDeclaration";
            string sql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupPath}' WITH FORMAT, INIT";

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await LogAudit("Backup", $"Бэкап БД {dbName} в {backupPath}");
                TempData["Result"] = "Резервное копирование завершено успешно!";
            }
            catch (Exception ex)
            {
                TempData["Result"] = "Ошибка: " + ex.Message;
            }
            return RedirectToAction("Backup");
        }

        // --- ВОССТАНОВЛЕНИЕ ---
        [HttpGet]
        public IActionResult Restore()
        {
            ViewBag.Result = TempData["Result"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string backupPath)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                backupPath = @"C:\Users\khaly\OneDrive\Desktop\TaxDeclaration.bak";

            string dbName = "TaxDeclaration";
            string sql = $@"
                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{dbName}] FROM DISK = N'{backupPath}' WITH REPLACE;
                ALTER DATABASE [{dbName}] SET MULTI_USER;
            ";

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await LogAudit("Restore", $"Восстановление БД {dbName} из {backupPath}");
                TempData["Result"] = "Восстановление завершено успешно!";
            }
            catch (Exception ex)
            {
                TempData["Result"] = "Ошибка: " + ex.Message;
            }
            return RedirectToAction("Restore");
        }


        // --- Логирование аудита ---
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

        // --- Логирование транзакций ---
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
    }
}
