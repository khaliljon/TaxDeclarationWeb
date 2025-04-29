using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using System;

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

        public IActionResult Create()
        {
            int nextCode = _context.Nationalities.Any()
                ? _context.Nationalities.Max(n => n.Code) + 1
                : 1;

            return View(new Nationality { Code = nextCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nationality nationality)
        {
            if (!ModelState.IsValid)
                return View(nationality);

            _context.Nationalities.Add(nationality);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var nationality = await _context.Nationalities.FindAsync(id);
            if (nationality == null) return NotFound();
            return View(nationality);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Nationality nationality)
        {
            if (!ModelState.IsValid)
                return View(nationality);

            try
            {
                _context.Update(nationality);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Nationalities.AnyAsync(n => n.Code == nationality.Code))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var nationality = await _context.Nationalities
                .FirstOrDefaultAsync(n => n.Code == id);
            if (nationality == null) return NotFound();
            return View(nationality);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var nationality = await _context.Nationalities
                .FirstOrDefaultAsync(n => n.Code == id);
            if (nationality == null) return NotFound();
            return View(nationality);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Nationality model)
        {
            try
            {
                var nationality = await _context.Nationalities.FindAsync(model.Code);
                if (nationality == null) return NotFound();

                _context.Nationalities.Remove(nationality);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Невозможно удалить национальность — она используется в других записях.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
