using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireChiefInspector")]
public class ChiefInspectorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChiefInspectorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TaxpayerCount = await _context.Taxpayers.CountAsync();
        ViewBag.DeclarationCount = await _context.Declarations.CountAsync();
        ViewBag.InspectorCount = await _context.Inspectors.CountAsync();
        ViewBag.InspectionCount = await _context.Inspections.CountAsync();

        return View();
    }

    public async Task<IActionResult> Logout([FromServices] SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Taxpayers()
    {
        return RedirectToAction("Index", "Taxpayer");
    }

    public IActionResult Declarations()
    {
        return RedirectToAction("Index", "Declaration");
    }

    public IActionResult Inspectors()
    {
        return RedirectToAction("Index", "Inspector");
    }

    public IActionResult Inspections()
    {
        return RedirectToAction("Index", "Inspection");
    }

    public IActionResult Categories()
    {
        return RedirectToAction("Index", "Category");
    }

    public IActionResult Countries()
    {
        return RedirectToAction("Index", "Country");
    }

    public IActionResult Nationalities()
    {
        return RedirectToAction("Index", "Nationality");
    }

    public IActionResult Report()
    {
        return RedirectToAction("Index", "Report");
    }
}
