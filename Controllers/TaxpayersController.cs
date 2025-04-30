using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "Inspector,ChiefInspector,Admin")]
    public class TaxpayersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaxpayersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            var query = _context.Taxpayers
                .Include(t => t.Inspection)
                .Include(t => t.Category)
                .Include(t => t.Country)
                .Include(t => t.Nationality)
                .AsQueryable();

            if (roles.Contains("Inspector"))
            {
                var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                if (inspector == null) return Forbid();

                query = query.Where(t => t.InspectionCode == inspector.InspectionCode);
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var isInspector = await _userManager.IsInRoleAsync(user, "Inspector");

            if (isInspector)
            {
                var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                if (inspector == null) return Forbid();

                ViewBag.InspectionCode = inspector.InspectionCode;
                ViewBag.InspectionName = inspector.Inspection?.Name ??
                                         (await _context.Inspections.FindAsync(inspector.InspectionCode))?.Name ?? "Неизвестно";
            }
            else
            {
                ViewBag.InspectionCode = new SelectList(_context.Inspections, "Code", "Name");
            }

            LoadOtherSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Taxpayer taxpayer)
        {
            var user = await _userManager.GetUserAsync(User);
            var isInspector = await _userManager.IsInRoleAsync(user, "Inspector");

            if (isInspector)
            {
                var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                if (inspector == null || taxpayer.InspectionCode != inspector.InspectionCode)
                    return Forbid();
            }

            if (!ModelState.IsValid)
            {
                LoadSelectLists(taxpayer);
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

            if (await AccessDeniedForInspector(taxpayer)) return Forbid();

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Inspector"))
            {
                var inspector = await _context.Inspectors
                    .Include(i => i.Inspection)
                    .FirstOrDefaultAsync(i => i.UserId == user.Id);

                ViewData["InspectionCode"] = new SelectList(
                    _context.Inspections.Where(i => i.Code == inspector.InspectionCode),
                    "Code", "Name", taxpayer.InspectionCode);
                ViewData["InspectionName"] = inspector.Inspection.Name;
            }
            else
            {
                ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", taxpayer.InspectionCode);
            }

            ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name", taxpayer.CategoryCode);
            ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name", taxpayer.CountryCode);
            ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name", taxpayer.NationalityCode);

            return View(taxpayer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Taxpayer taxpayer)
        {
            if (!ModelState.IsValid)
            {
                LoadSelectLists(taxpayer);
                return View(taxpayer);
            }

            if (await AccessDeniedForInspector(taxpayer)) return Forbid();

            try
            {
                _context.Update(taxpayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxpayerExists(taxpayer.IIN)) return NotFound();
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
            if (await AccessDeniedForInspector(taxpayer)) return Forbid();

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
            if (await AccessDeniedForInspector(taxpayer)) return Forbid();

            return View(taxpayer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Taxpayer model)
        {
            var taxpayer = await _context.Taxpayers.FindAsync(model.IIN);
            if (taxpayer == null) return NotFound();
            if (await AccessDeniedForInspector(taxpayer)) return Forbid();

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

        private bool TaxpayerExists(string id) =>
            _context.Taxpayers.Any(e => e.IIN == id);

        private async Task<bool> AccessDeniedForInspector(Taxpayer taxpayer)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsInRoleAsync(user, "Inspector")) return false;

            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            return inspector == null || inspector.InspectionCode != taxpayer.InspectionCode;
        }

        private void LoadSelectLists(Taxpayer taxpayer = null)
        {
            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", taxpayer?.InspectionCode);
            LoadOtherSelectLists(taxpayer);
        }

        private void LoadOtherSelectLists(Taxpayer taxpayer = null)
        {
            ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name", taxpayer?.CategoryCode);
            ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name", taxpayer?.CountryCode);
            ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name", taxpayer?.NationalityCode);
        }
    }
}
