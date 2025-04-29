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
    public class CountryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CountryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.ToListAsync());
        }

        public IActionResult Create()
        {
            int nextCode = _context.Countries.Any()
                ? _context.Countries.Max(c => c.Code) + 1
                : 1;

            return View(new Country { Code = nextCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            if (!ModelState.IsValid)
                return View(country);

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound();
            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Country country)
        {
            if (!ModelState.IsValid)
                return View(country);

            try
            {
                _context.Update(country);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Countries.AnyAsync(c => c.Code == country.Code))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Code == id);
            if (country == null) return NotFound();
            return View(country);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Code == id);
            if (country == null) return NotFound();
            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Country model)
        {
            try
            {
                var country = await _context.Countries.FindAsync(model.Code);
                if (country == null) return NotFound();

                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Невозможно удалить страну — она связана с другими записями.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
