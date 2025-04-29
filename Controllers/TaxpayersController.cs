using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "ChiefInspector,Admin")]
    public class TaxpayersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaxpayersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var taxpayers = await _context.Taxpayers
                .Include(t => t.Inspection)
                .Include(t => t.Category)
                .Include(t => t.Country)
                .Include(t => t.Nationality)
                .ToListAsync();

            return View(taxpayers);
        }

        public IActionResult Create()
        {
            LoadSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Taxpayer taxpayer)
        {
            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return View(taxpayer);
            }

            _context.Add(taxpayer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var taxpayer = await _context.Taxpayers.FindAsync(id);
            if (taxpayer == null) return NotFound();

            LoadSelectLists(taxpayer);
            return View(taxpayer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Taxpayer taxpayer)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Ошибка валидации при редактировании налогоплательщика.");
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Поле: {entry.Key} — Ошибка: {error.ErrorMessage}");
                    }
                }

                LoadSelectLists(taxpayer);
                return View(taxpayer);
            }

            try
            {
                _context.Update(taxpayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxpayerExists(taxpayer.IIN))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var taxpayer = await _context.Taxpayers
                .Include(t => t.Inspection)
                .Include(t => t.Category)
                .Include(t => t.Country)
                .Include(t => t.Nationality)
                .FirstOrDefaultAsync(t => t.IIN == id);

            if (taxpayer == null) return NotFound();
            return View(taxpayer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var taxpayer = await _context.Taxpayers
                .Include(t => t.Inspection)
                .Include(t => t.Category)
                .Include(t => t.Country)
                .Include(t => t.Nationality)
                .FirstOrDefaultAsync(t => t.IIN == id);

            if (taxpayer == null) return NotFound();
            return View(taxpayer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Taxpayer model)
        {
            var taxpayer = await _context.Taxpayers.FindAsync(model.IIN);
            if (taxpayer == null) return NotFound();

            try
            {
                _context.Taxpayers.Remove(taxpayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Невозможно удалить налогоплательщика — он связан с другими данными.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TaxpayerExists(string id)
        {
            return _context.Taxpayers.Any(e => e.IIN == id);
        }

        private void LoadSelectLists(Taxpayer taxpayer = null)
        {
            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", taxpayer?.InspectionCode);
            ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name", taxpayer?.CategoryCode);
            ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name", taxpayer?.CountryCode);
            ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name", taxpayer?.NationalityCode);
        }
    }
}
