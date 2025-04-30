using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "Taxpayer")]
public class TaxpayerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaxpayerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var taxpayer = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Nationality)
            .Include(t => t.Country)
            .FirstOrDefaultAsync(t => t.IIN == user.IIN);

        if (taxpayer == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(taxpayer);
    }
}