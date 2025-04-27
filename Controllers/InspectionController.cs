using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "ChiefInspector,Admin")]
    public class InspectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InspectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Inspections.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Code == id);
            if (inspection == null) return NotFound();
            return View(inspection);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,Address")] Inspection inspection)
        {
            if (!ModelState.IsValid)
                return View(inspection);

            _context.Add(inspection);
            await _context.SaveChangesAsync();

            await LogTransaction(
                "Insert",
                "Inspection",
                inspection.Code.ToString(),
                $"Добавлена налоговая инспекция: {inspection.Name} (код: {inspection.Code})"
            );

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return NotFound();
            return View(inspection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,Name,Address")] Inspection inspection)
        {
            if (id != inspection.Code) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection);
                    await _context.SaveChangesAsync();

                    await LogTransaction(
                        "Update",
                        "Inspection",
                        inspection.Code.ToString(),
                        $"Изменена налоговая инспекция: {inspection.Name} (код: {inspection.Code})"
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Inspections.Any(e => e.Code == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(inspection);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Code == id);
            if (inspection == null) return NotFound();

            return View(inspection);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Delete",
                    "Inspection",
                    inspection.Code.ToString(),
                    $"Удалена налоговая инспекция: {inspection.Name} (код: {inspection.Code})"
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
