using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers
{
    [Authorize(Roles = "Inspector")]
    public class InspectorRoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InspectorRoleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (inspector == null)
                return Forbid();

            ViewBag.TaxpayerCount =
                await _context.Taxpayers.CountAsync(t => t.InspectionCode == inspector.InspectionCode);
            ViewBag.DeclarationCount =
                await _context.Declarations.CountAsync(d => d.InspectionId == inspector.InspectionCode);
            ViewBag.InspectionName = inspector.Inspection?.Name ?? "";

            return View();
        }

        public async Task<IActionResult> Taxpayers()
        {
            var user = await _userManager.GetUserAsync(User);
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (inspector == null)
                return Forbid();

            var taxpayers = await _context.Taxpayers
                .Where(t => t.InspectionCode == inspector.InspectionCode)
                .ToListAsync();

            return View(taxpayers);
        }

        public async Task<IActionResult> Declarations()
        {
            var user = await _userManager.GetUserAsync(User);
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (inspector == null)
                return Forbid();

            var declarations = await _context.Declarations
                .Where(d => d.InspectionId == inspector.InspectionCode)
                .ToListAsync();

            return View(declarations);
        }

        public async Task<IActionResult> NonResidentMandatoryDeclarations()
        {
            var user = await _userManager.GetUserAsync(User);
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (inspector == null)
                return Forbid();

            var list = await _context.Taxpayers
                .Where(t => t.InspectionCode == inspector.InspectionCode
                            && !t.IsResident
                            && t.IsDeclarationRequired)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Logout([FromServices] SignInManager<ApplicationUser> signInManager)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }

}
