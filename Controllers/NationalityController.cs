using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "ChiefInspector,Admin")]
    public class NationalityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NationalityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Nationalities.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var nationality = await _context.Nationalities.FirstOrDefaultAsync(n => n.Code == id);
            if (nationality == null)
                return NotFound();
            return View(nationality);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name")] Nationality nationality)
        {
            if (!ModelState.IsValid)
                return View(nationality);

            _context.Add(nationality);
            await _context.SaveChangesAsync();

            await LogTransaction(
                "Insert",
                "Nationality",
                nationality.Code.ToString(),
                $"Добавлена национальность: {nationality.Name} (код: {nationality.Code})"
            );

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var nationality = await _context.Nationalities.FindAsync(id);
            if (nationality == null)
                return NotFound();
            return View(nationality);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,Name")] Nationality nationality)
        {
            if (id != nationality.Code)
                return NotFound();

            if (!ModelState.IsValid)
                return View(nationality);

            try
            {
                _context.Update(nationality);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Update",
                    "Nationality",
                    nationality.Code.ToString(),
                    $"Изменена национальность: {nationality.Name} (код: {nationality.Code})"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Nationalities.Any(n => n.Code == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var nationality = await _context.Nationalities.FirstOrDefaultAsync(n => n.Code == id);
            if (nationality == null)
                return NotFound();
            return View(nationality);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nationality = await _context.Nationalities.FindAsync(id);
            if (nationality != null)
            {
                _context.Nationalities.Remove(nationality);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Delete",
                    "Nationality",
                    nationality.Code.ToString(),
                    $"Удалена национальность: {nationality.Name} (код: {nationality.Code})"
                );
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Приватный метод для логирования транзакций ---
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
