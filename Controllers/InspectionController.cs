using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using System.Linq;
using System;

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
            int nextCode = _context.Inspections.Any()
                ? _context.Inspections.Max(i => i.Code) + 1
                : 1;

            return View(new Inspection { Code = nextCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inspection inspection)
        {
            if (!ModelState.IsValid)
                return View(inspection);

            if (await _context.Inspections.AnyAsync(i => i.Code == inspection.Code))
            {
                ModelState.AddModelError("Code", "Инспекция с таким кодом уже существует.");
                return View(inspection);
            }

            _context.Add(inspection);
            await _context.SaveChangesAsync();

            await LogTransaction("Insert", "Inspection", inspection.Code.ToString(),
                $"Добавлена инспекция: {inspection.Name} (код: {inspection.Code})");

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
        public async Task<IActionResult> Edit(Inspection inspection)
        {
            if (!ModelState.IsValid)
                return View(inspection);

            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();

                await LogTransaction("Update", "Inspection", inspection.Code.ToString(),
                    $"Изменена инспекция: {inspection.Name} (код: {inspection.Code})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Inspections.AnyAsync(i => i.Code == inspection.Code))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Code == id);
            if (inspection == null) return NotFound();
            return View(inspection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Inspection model)
        {
            try
            {
                var inspection = await _context.Inspections.FindAsync(model.Code);
                if (inspection == null)
                    return NotFound();

                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();

                await LogTransaction("Delete", "Inspection", inspection.Code.ToString(),
                    $"Удалена инспекция: {inspection.Name} (код: {inspection.Code})");

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Невозможно удалить инспекцию — она связана с другими данными.";
                return RedirectToAction(nameof(Index));
            }
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
