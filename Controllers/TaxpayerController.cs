using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireTaxpayer")]
public class TaxpayerController : Controller
{
    private readonly ApplicationDbContext _context;

    public TaxpayerController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var taxpayers = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Country)
            .Include(t => t.Nationality)
            .ToListAsync();

        return View(taxpayers);
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

        return View(taxpayer);
    }

    public IActionResult Create()
    {
        ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name");
        ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name");
        ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name");
        ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Taxpayer taxpayer)
    {
        if (!ModelState.IsValid)
        {
            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", taxpayer.InspectionCode);
            ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name", taxpayer.CategoryCode);
            ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name", taxpayer.CountryCode);
            ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name", taxpayer.NationalityCode);
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

        ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", taxpayer.InspectionCode);
        ViewData["CategoryCode"] = new SelectList(_context.Categories, "Code", "Name", taxpayer.CategoryCode);
        ViewData["CountryCode"] = new SelectList(_context.Countries, "Code", "Name", taxpayer.CountryCode);
        ViewData["NationalityCode"] = new SelectList(_context.Nationalities, "Code", "Name", taxpayer.NationalityCode);

        return View(taxpayer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Taxpayer taxpayer)
    {
        if (id != taxpayer.IIN) return NotFound();

        if (!ModelState.IsValid)
        {
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

        return View(taxpayer);
    }

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
