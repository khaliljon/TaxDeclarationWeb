using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "Taxpayer,Inspector,ChiefInspector,Admin")]
public class DeclarationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeclarationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        var declarations = _context.Declarations
            .Include(d => d.Inspection)
            .Include(d => d.Inspector)
            .Include(d => d.Taxpayer)
            .AsQueryable();

        if (roles.Contains("Inspector"))
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            if (inspector == null) return Forbid();
            declarations = declarations.Where(d => d.InspectionId == inspector.InspectionCode);
        }
        else if (roles.Contains("Taxpayer"))
        {
            declarations = declarations.Where(d => d.TaxpayerIIN == user.IIN);
        }

        return View(await declarations.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        var declaration = new Declaration
        {
            SubmittedAt = DateTime.Today,
            Year = DateTime.Today.Year
        };

        if (roles.Contains("Taxpayer"))
        {
            declaration.TaxpayerIIN = user.IIN;
        }

        if (roles.Contains("Inspector"))
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            if (inspector == null) return Forbid();

            declaration.InspectorId = inspector.Code;
            declaration.InspectionId = inspector.InspectionCode;
        }

        LoadSelectLists(declaration, roles, user);
        return View(declaration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Declaration declaration)
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Taxpayer"))
        {
            declaration.TaxpayerIIN = user.IIN;
        }

        if (roles.Contains("Inspector"))
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            if (inspector == null || declaration.InspectionId != inspector.InspectionCode)
                return Forbid();

            declaration.InspectorId = inspector.Code;
        }

        if (!ModelState.IsValid)
        {
            LoadSelectLists(declaration, roles, user);
            return View(declaration);
        }

        _context.Add(declaration);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var declaration = await _context.Declarations.FindAsync(id);
        if (declaration == null) return NotFound();

        if (await AccessDenied(declaration)) return Forbid();

        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        LoadSelectLists(declaration, roles, user);
        return View(declaration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Declaration declaration)
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        if (!ModelState.IsValid)
        {
            LoadSelectLists(declaration, roles, user);
            return View(declaration);
        }

        if (await AccessDenied(declaration)) return Forbid();

        if (roles.Contains("Inspector"))
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            declaration.InspectorId = inspector.Code;
            declaration.InspectionId = inspector.InspectionCode;
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
        if (await AccessDenied(declaration)) return Forbid();

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
        if (await AccessDenied(declaration)) return Forbid();

        return View(declaration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Declaration model)
    {
        var declaration = await _context.Declarations.FindAsync(model.Id);
        if (declaration == null) return NotFound();
        if (await AccessDenied(declaration)) return Forbid();

        try
        {
            _context.Declarations.Remove(declaration);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Невозможно удалить декларацию — она связана с другими данными.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> AccessDenied(Declaration declaration)
    {
        var user = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(user, "Taxpayer"))
            return declaration.TaxpayerIIN != user.IIN;

        if (await _userManager.IsInRoleAsync(user, "Inspector"))
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            return inspector == null || inspector.InspectionCode != declaration.InspectionId;
        }

        return false;
    }

    private void LoadSelectLists(Declaration declaration, IList<string> roles, ApplicationUser user)
    {
        if (roles.Contains("Taxpayer"))
        {
            ViewData["TaxpayerIIN"] = new SelectList(new[] { new { IIN = user.IIN, FullName = user.Email } }, "IIN", "FullName", declaration.TaxpayerIIN);
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
            ViewData["InspectorId"] = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
        }
        else
        {
            ViewData["TaxpayerIIN"] = new SelectList(_context.Taxpayers, "IIN", "FullName", declaration.TaxpayerIIN);
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Code", "Name", declaration.InspectionId);
            ViewData["InspectorId"] = new SelectList(_context.Inspectors, "Code", "FullName", declaration.InspectorId);
        }
    }
}
