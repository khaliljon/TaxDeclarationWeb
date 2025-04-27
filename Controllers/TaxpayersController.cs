using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireTaxpayer")]
public class TaxpayersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaxpayersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Taxpayers
    public async Task<IActionResult> Index()
    {
        var taxpayers = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Nationality)
            .Include(t => t.Country)
            .ToListAsync();
        return View(taxpayers);
    }

    // GET: Taxpayers/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
            return NotFound();

        var taxpayer = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Nationality)
            .Include(t => t.Country)
            .FirstOrDefaultAsync(t => t.IIN == id);

        if (taxpayer == null)
            return NotFound();

        return View(taxpayer);
    }

    // GET: Taxpayers/Create
    public IActionResult Create()
    {
        ViewBag.Inspections = new SelectList(_context.Inspections, "Code", "Name");
        ViewBag.Categories = new SelectList(_context.Categories, "Code", "Name");
        ViewBag.Nationalities = new SelectList(_context.Nationalities, "Code", "Name");
        ViewBag.Countries = new SelectList(_context.Countries, "Code", "Name");
        return View();
    }

    // POST: Taxpayers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IIN,FullName,Address,Phone,InspectionCode,CategoryCode,IsDeclarationRequired,BirthDate,Gender,NationalityCode,Workplace,IsResident,CountryCode")] Taxpayer taxpayer)
    {
        if (ModelState.IsValid)
        {
            _context.Add(taxpayer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Inspections = new SelectList(_context.Inspections, "Code", "Name", taxpayer.InspectionCode);
        ViewBag.Categories = new SelectList(_context.Categories, "Code", "Name", taxpayer.CategoryCode);
        ViewBag.Nationalities = new SelectList(_context.Nationalities, "Code", "Name", taxpayer.NationalityCode);
        ViewBag.Countries = new SelectList(_context.Countries, "Code", "Name", taxpayer.CountryCode);
        return View(taxpayer);
    }

    // GET: Taxpayers/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var taxpayer = await _context.Taxpayers.FindAsync(id);
        if (taxpayer == null) return NotFound();

        ViewBag.Inspections = new SelectList(_context.Inspections, "Code", "Name", taxpayer.InspectionCode);
        ViewBag.Categories = new SelectList(_context.Categories, "Code", "Name", taxpayer.CategoryCode);
        ViewBag.Nationalities = new SelectList(_context.Nationalities, "Code", "Name", taxpayer.NationalityCode);
        ViewBag.Countries = new SelectList(_context.Countries, "Code", "Name", taxpayer.CountryCode);
        return View(taxpayer);
    }

    // POST: Taxpayers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("IIN,FullName,Address,Phone,InspectionCode,CategoryCode,IsDeclarationRequired,BirthDate,Gender,NationalityCode,Workplace,IsResident,CountryCode")] Taxpayer taxpayer)
    {
        if (id != taxpayer.IIN) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(taxpayer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Taxpayers.Any(e => e.IIN == id))
                    return NotFound();
                else
                    throw;
            }
        }
        ViewBag.Inspections = new SelectList(_context.Inspections, "Code", "Name", taxpayer.InspectionCode);
        ViewBag.Categories = new SelectList(_context.Categories, "Code", "Name", taxpayer.CategoryCode);
        ViewBag.Nationalities = new SelectList(_context.Nationalities, "Code", "Name", taxpayer.NationalityCode);
        ViewBag.Countries = new SelectList(_context.Countries, "Code", "Name", taxpayer.CountryCode);
        return View(taxpayer);
    }

    // GET: Taxpayers/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var taxpayer = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Nationality)
            .Include(t => t.Country)
            .FirstOrDefaultAsync(t => t.IIN == id);

        if (taxpayer == null) return NotFound();

        return View(taxpayer);
    }

    // POST: Taxpayers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var taxpayer = await _context.Taxpayers.FindAsync(id);
        if (taxpayer != null)
        {
            _context.Taxpayers.Remove(taxpayer);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
