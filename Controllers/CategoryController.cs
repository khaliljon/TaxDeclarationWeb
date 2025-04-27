using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Policy = "RequireChiefInspector")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Code == id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name")] Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _context.Add(category);
            await _context.SaveChangesAsync();

            await LogTransaction(
                "Insert",
                "Category",
                category.Code.ToString(),
                $"Создана категория плательщика: {category.Name} (код: {category.Code})"
            );

            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,Name")] Category category)
        {
            if (id != category.Code)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Update",
                    "Category",
                    category.Code.ToString(),
                    $"Изменена категория плательщика: {category.Name} (код: {category.Code})"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.Code == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Code == id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                await LogTransaction(
                    "Delete",
                    "Category",
                    category.Code.ToString(),
                    $"Удалена категория плательщика: {category.Name} (код: {category.Code})"
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
