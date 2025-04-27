using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Policy = "RequireChiefInspector")]
    public class CountryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CountryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Countries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.ToListAsync());
        }

        // GET: Countries/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Code == id);
            if (country == null)
                return NotFound();

            return View(country);
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name")] Country country)
        {
            if (!ModelState.IsValid)
                return View(country);

            _context.Add(country);
            await _context.SaveChangesAsync();

            await LogTransaction(
                "Insert",
                "Country",
                country.Code.ToString(),
                $"Добавлена страна: {country.Name} (код: {country.Code})"
            );

            return RedirectToAction(nameof(Index));
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
                return NotFound();

            return View(country);
        }

        // POST: Countries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,Name")] Country country)
        {
            if (id != country.Code)
                return NotFound();

            if (!ModelState.IsValid)
                return View(country);

            try
            {
                _context.Update(country);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Update",
                    "Country",
                    country.Code.ToString(),
                    $"Изменена страна: {country.Name} (код: {country.Code})"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Countries.Any(e => e.Code == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Code == id);
            if (country == null)
                return NotFound();

            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country != null)
            {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Delete",
                    "Country",
                    country.Code.ToString(),
                    $"Удалена страна: {country.Name} (код: {country.Code})"
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
