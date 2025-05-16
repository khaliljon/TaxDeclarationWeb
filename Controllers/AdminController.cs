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
using System.IO;

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
        
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout([FromServices] SignInManager<ApplicationUser> signInManager)
        {
            await LogAudit("Выход администратора", $"Выход выполнен: {User.Identity?.Name}");
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

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

        public async Task<IActionResult> Audit()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }

        public async Task<IActionResult> TransactionLog()
        {
            var logs = await _context.TransactionLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }

[HttpPost]
public IActionResult CreateBackup()
{
    string backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
    if (!Directory.Exists(backupDirectory))
        Directory.CreateDirectory(backupDirectory);

    string backupFile = $"TaxDeclaration_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
    string fullPath = Path.Combine(backupDirectory, backupFile);

    string sql = $@"BACKUP DATABASE [TaxDeclaration] TO DISK = N'{fullPath}' WITH INIT, FORMAT";

    // Всегда только из .env
    string connString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    Console.WriteLine($"[BACKUP] connString: {connString}");

    try
    {
        using (var connection = new SqlConnection(connString))
        {
            connection.Open();
            using (var command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        TempData["Message"] = "Резервная копия успешно создана.";
    }
    catch (Exception ex)
    {
        TempData["Message"] = $"Ошибка при создании резервной копии: {ex.Message}";
    }

    return RedirectToAction("Backups");
}

public IActionResult Backups()
{
    string backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
    var files = Directory.Exists(backupDirectory)
        ? Directory.GetFiles(backupDirectory)
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList()
        : new List<FileInfo>();

    ViewBag.Message = TempData["Message"];
    return View(files); // files — List<FileInfo>
}

[HttpPost]
public IActionResult DeleteBackup(string fileName)
{
    string backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
    var path = Path.Combine(backupDirectory, fileName);

    if (System.IO.File.Exists(path))
    {
        System.IO.File.Delete(path);
        TempData["Message"] = "Резервная копия успешно удалена.";
    }
    else
    {
        TempData["Message"] = "Ошибка: файл не найден.";
    }

    return RedirectToAction("Backups");
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult RestoreDatabase(string fileName)
{
    string backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
    string fullPath = Path.Combine(backupDirectory, fileName);

    if (!System.IO.File.Exists(fullPath))
    {
        TempData["Message"] = "Ошибка: файл резервной копии не найден.";
        return RedirectToAction("Restore");
    }

    string sql = $@"RESTORE DATABASE [TaxDeclaration] FROM DISK = N'{fullPath}' WITH REPLACE";

    string connString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    Console.WriteLine($"[RESTORE] connString: {connString}");

    try
    {
        using (var connection = new SqlConnection(connString))
        {
            connection.Open();
            using (var command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        TempData["Message"] = "База данных успешно восстановлена из резервной копии.";
    }
    catch (Exception ex)
    {
        TempData["Message"] = $"Ошибка при восстановлении базы данных: {ex.Message}";
    }

    return RedirectToAction("Restore");
}

[HttpGet]
public IActionResult Restore()
{
    string backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
    var files = Directory.Exists(backupDirectory)
        ? Directory.GetFiles(backupDirectory)
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList()
        : new List<FileInfo>();

    return View(files); 
}

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
