using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireInspector")]
public class InspectorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InspectorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var totalTaxpayers = await _context.Taxpayers.CountAsync();
        var totalDeclarations = await _context.Declarations.CountAsync();

        ViewBag.TotalTaxpayers = totalTaxpayers;
        ViewBag.TotalDeclarations = totalDeclarations;

        var maskedUsers = await _context.Taxpayers
            .Select(t => new
            {
                IIN = t.IIN.Substring(0, 4) + "******", // маскируем
                FullName = t.FullName,
                Phone = t.Phone.Substring(0, 4) + "****", // маскируем
                Address = t.Address.Length > 10 ? t.Address.Substring(0, 10) + "..." : t.Address
            })
            .Take(3)
            .ToListAsync();

        ViewBag.MaskedSample = maskedUsers;

        return View();
    }

    public async Task<IActionResult> Logout([FromServices] SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}