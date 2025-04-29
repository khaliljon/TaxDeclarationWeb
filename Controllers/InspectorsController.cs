using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using System;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "ChiefInspector,Admin")]
    public class InspectorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InspectorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var inspectors = await _context.Inspectors
                .Include(i => i.Inspection)
                .ToListAsync();

            return View(inspectors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inspector = await _context.Inspectors
                .Include(i => i.Inspection)
                .FirstOrDefaultAsync(i => i.Code == id);

            if (inspector == null) return NotFound();
            return View(inspector);
        }

        public IActionResult Create()
        {
            var nextCode = _context.Inspectors.Any()
                ? _context.Inspectors.Max(i => i.Code) + 1
                : 1;

            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name");
            return View(new Inspector { Code = nextCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inspector inspector)
        {
            if (!ModelState.IsValid)
            {
                ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
                return View(inspector);
            }

            inspector.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(inspector.UserId))
            {
                ModelState.AddModelError("", "Ошибка: невозможно определить текущего пользователя.");
                ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
                return View(inspector);
            }

            if (await _context.Inspectors.AnyAsync(i => i.Code == inspector.Code))
            {
                ModelState.AddModelError("Code", "Инспектор с таким кодом уже существует.");
                ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
                return View(inspector);
            }

            _context.Inspectors.Add(inspector);
            await _context.SaveChangesAsync();

            await LogTransaction("Insert", "Inspector", inspector.Code.ToString(),
                $"Добавлен инспектор: {inspector.FullName} (код: {inspector.Code})");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inspector = await _context.Inspectors.FindAsync(id);
            if (inspector == null) return NotFound();

            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
            return View(inspector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inspector inspector)
        {
            if (!ModelState.IsValid)
            {
                ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
                return View(inspector);
            }

            try
            {
                // ВАЖНО: подгружаем оригинальный UserId
                var existing = await _context.Inspectors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Code == inspector.Code);

                if (existing == null)
                    return NotFound();

                inspector.UserId = existing.UserId;

                _context.Update(inspector);
                await _context.SaveChangesAsync();

                await LogTransaction("Update", "Inspector", inspector.Code.ToString(),
                    $"Изменён инспектор: {inspector.FullName} (код: {inspector.Code})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Inspectors.AnyAsync(i => i.Code == inspector.Code))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var inspector = await _context.Inspectors
                .Include(i => i.Inspection)
                .FirstOrDefaultAsync(i => i.Code == id);

            if (inspector == null) return NotFound();
            return View(inspector);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Inspector model)
        {
            try
            {
                var inspector = await _context.Inspectors.FindAsync(model.Code);
                if (inspector == null) return NotFound();

                _context.Inspectors.Remove(inspector);
                await _context.SaveChangesAsync();

                await LogTransaction("Delete", "Inspector", inspector.Code.ToString(),
                    $"Удалён инспектор: {inspector.FullName} (код: {inspector.Code})");
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Невозможно удалить инспектора — он связан с другими данными.";
            }

            return RedirectToAction(nameof(Index));
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
