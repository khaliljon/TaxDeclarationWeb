using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "Taxpayer,Inspector,ChiefInspector,Admin")]
    public class TaxpayerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaxpayerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<IList<string>> GetCurrentUserRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            return user == null ? new List<string>() : await _userManager.GetRolesAsync(user);
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        public async Task<IActionResult> Index()
        {
            var roles = await GetCurrentUserRoles();
            var user = await GetCurrentUserAsync();

            IQueryable<Taxpayer> taxpayers = _context.Taxpayers
                .Include(t => t.Inspection)
                .Include(t => t.Category)
                .Include(t => t.Country)
                .Include(t => t.Nationality);

            if (roles.Contains("Taxpayer"))
            {
                // Предполагаем, что UserName это ИИН, иначе дорабатывай под user.IIN
                taxpayers = taxpayers.Where(t => t.IIN == user.UserName);
            }
            else if (roles.Contains("Inspector"))
            {
                var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                if (inspector != null)
                    taxpayers = taxpayers.Where(t => t.InspectionCode == inspector.InspectionCode);
                else
                    taxpayers = taxpayers.Where(t => false);
            }
            // ChiefInspector и Admin — видят всё

            return View(await taxpayers.ToListAsync());
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

            if (!await CanAccess(taxpayer))
                return Forbid();

            return View(taxpayer);
        }

        [Authorize(Roles = "Inspector,ChiefInspector,Admin")]
        public IActionResult Create()
        {
            SetViewData(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Inspector,ChiefInspector,Admin")]
        public async Task<IActionResult> Create(Taxpayer taxpayer)
        {
            if (!ModelState.IsValid)
            {
                SetViewData(taxpayer);
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

            if (!await CanAccess(taxpayer))
                return Forbid();

            SetViewData(taxpayer);
            return View(taxpayer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Taxpayer taxpayer)
        {
            if (id != taxpayer.IIN) return NotFound();

            if (!await CanAccess(taxpayer))
                return Forbid();

            if (!ModelState.IsValid)
            {
                SetViewData(taxpayer);
                return View(taxpayer);
            }

            try
            {
                _context.Update(taxpayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Taxpayers.Any(e => e.IIN == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
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

            if (!await CanAccess(taxpayer))
                return Forbid();

            return View(taxpayer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var taxpayer = await _context.Taxpayers.FindAsync(id);

            if (!await CanAccess(taxpayer))
                return Forbid();

            if (taxpayer != null)
            {
                _context.Taxpayers.Remove(taxpayer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // --- Вспомогательные методы ---

        private async Task<bool> CanAccess(Taxpayer taxpayer)
        {
            var roles = await GetCurrentUserRoles();
            var user = await GetCurrentUserAsync();

            if (roles.Contains("Admin") || roles.Contains("ChiefInspector"))
                return true;

            if (roles.Contains("Taxpayer"))
                return taxpayer.IIN == user.UserName; // Или user.IIN, если реализуешь отдельное поле

            if (roles.Contains("Inspector"))
            {
                var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                return inspector != null && taxpayer.InspectionCode == inspector.InspectionCode;
            }
            return false;
        }

        private void SetViewData(Taxpayer taxpayer)
        {
            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", taxpayer?.InspectionCode);
            ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name", taxpayer?.CategoryCode);
            ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name", taxpayer?.CountryCode);
            ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name", taxpayer?.NationalityCode);
        }
    }
}
