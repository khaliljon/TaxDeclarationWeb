using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireChiefInspector")]
public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InspectionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Inspections
    public async Task<IActionResult> Index()
    {
        var inspections = await _context.Inspections.ToListAsync();
        return View(inspections);
    }

    // GET: Inspections/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Code == id);
        if (inspection == null) return NotFound();

        return View(inspection);
    }

    // GET: Inspections/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Inspections/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Name,Address")] Inspection inspection)
    {
        if (!ModelState.IsValid)
            return View(inspection);

        _context.Add(inspection);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Inspections/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection == null) return NotFound();

        return View(inspection);
    }

    // POST: Inspections/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Code,Name,Address")] Inspection inspection)
    {
        if (id != inspection.Code) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inspections.Any(e => e.Code == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(inspection);
    }

    // GET: Inspections/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Code == id);
        if (inspection == null) return NotFound();

        return View(inspection);
    }

    // POST: Inspections/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection != null)
        {
            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
