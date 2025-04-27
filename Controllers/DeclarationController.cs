using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "Taxpayer,Inspector,ChiefInspector,Admin")]
    public class DeclarationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeclarationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Declaration
        public async Task<IActionResult> Index()
        {
            var declarations = await _context.Declarations
                .Include(d => d.Taxpayer)
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .ToListAsync();

            return View(declarations);
        }

        // GET: Declaration/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var declaration = await _context.Declarations
                .Include(d => d.Taxpayer)
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (declaration == null) return NotFound();

            return View(declaration);
        }

        // GET: Declaration/Create
        public IActionResult Create()
        {
            ViewBag.TaxpayerIIN = new SelectList(_context.Taxpayers, "IIN", "FullName");
            ViewBag.InspectionId = new SelectList(_context.Inspections, "Code", "Name");
            ViewBag.InspectorId = new SelectList(_context.Inspectors, "Code", "FullName");
            return View();
        }

        // POST: Declaration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Declaration declaration)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TaxpayerIIN = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);
                ViewBag.InspectionId = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
                ViewBag.InspectorId = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
                return View(declaration);
            }

            _context.Add(declaration);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Declaration/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var declaration = await _context.Declarations.FindAsync(id);
            if (declaration == null) return NotFound();

            ViewBag.TaxpayerIIN = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);
            ViewBag.InspectionId = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
            ViewBag.InspectorId = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
            return View(declaration);
        }

        // POST: Declaration/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Declaration declaration)
        {
            if (id != declaration.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.TaxpayerIIN = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);
                ViewBag.InspectionId = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
                ViewBag.InspectorId = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
                return View(declaration);
            }

            try
            {
                _context.Update(declaration);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Declarations.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Declaration/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var declaration = await _context.Declarations
                .Include(d => d.Taxpayer)
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (declaration == null) return NotFound();

            return View(declaration);
        }

        // POST: Declaration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var declaration = await _context.Declarations.FindAsync(id);
            if (declaration != null)
            {
                _context.Declarations.Remove(declaration);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
