using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "ChiefInspector,Admin")]
    public class DeclarationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeclarationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var declarations = await _context.Declarations
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .Include(d => d.Taxpayer)
                .ToListAsync();

            return View(declarations);
        }

        public IActionResult Create()
        {
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Code", "Name");
            ViewData["InspectorId"] = new SelectList(_context.Inspectors, "Code", "FullName");
            ViewData["TaxpayerIIN"] = new SelectList(_context.Taxpayers, "IIN", "FullName");

            return View(new Declaration
            {
                SubmittedAt = DateTime.Today,
                Year = DateTime.Today.Year
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Declaration declaration)
        {
            if (!ModelState.IsValid)
            {
                ViewData["InspectionId"] = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
                ViewData["InspectorId"] = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
                ViewData["TaxpayerIIN"] = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);
                return View(declaration);
            }

            _context.Declarations.Add(declaration);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var declaration = await _context.Declarations.FindAsync(id);
            if (declaration == null) return NotFound();

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
            ViewData["InspectorId"] = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
            ViewData["TaxpayerIIN"] = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);

            return View(declaration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Declaration declaration)
        {
            if (!ModelState.IsValid)
            {
                ViewData["InspectionId"] = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
                ViewData["InspectorId"] = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
                ViewData["TaxpayerIIN"] = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);
                return View(declaration);
            }

            try
            {
                _context.Update(declaration);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Declarations.Any(d => d.Id == declaration.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var declaration = await _context.Declarations
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .Include(d => d.Taxpayer)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (declaration == null) return NotFound();
            return View(declaration);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var declaration = await _context.Declarations
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .Include(d => d.Taxpayer)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (declaration == null) return NotFound();
            return View(declaration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Declaration model)
        {
            var declaration = await _context.Declarations.FindAsync(model.Id);
            if (declaration == null) return NotFound();

            try
            {
                _context.Declarations.Remove(declaration);
                await _context.SaveChangesAsync();

                // Пересброс автоинкремента, если нужно
                var maxId = await _context.Declarations.AnyAsync()
                    ? await _context.Declarations.MaxAsync(d => d.Id)
                    : 0;

                await _context.Database.ExecuteSqlRawAsync(
                    "DBCC CHECKIDENT ('[Декларации]', RESEED, {0})", maxId);
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Невозможно удалить декларацию — она связана с другими записями.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
