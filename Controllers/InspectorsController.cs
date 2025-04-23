using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;


namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireAdmin")]
public class InspectorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InspectorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Inspectors
    public async Task<IActionResult> Index()
    {
        var inspectors = await _context.Inspectors
            .Include(i => i.Inspection)
            .ToListAsync();
        return View(inspectors);
    }

    // GET: Inspectors/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null) return NotFound();

        var inspector = await _context.Inspectors
            .Include(i => i.Inspection)
            .FirstOrDefaultAsync(i => i.Code == id);

        if (inspector == null) return NotFound();

        return View(inspector);
    }

    // GET: Inspectors/Create
    public IActionResult Create()
    {
        ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name");
        return View();
    }

    // POST: Inspectors/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,FullName,InspectionCode,Phone")] Inspector inspector)
    {
        if (!ModelState.IsValid)
        {
            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
            return View(inspector);
        }

        _context.Add(inspector);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Inspectors/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var inspector = await _context.Inspectors.FindAsync(id);
        if (inspector == null) return NotFound();

        ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
        return View(inspector);
    }

    // POST: Inspectors/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Code,FullName,InspectionCode,Phone")] Inspector inspector)
    {
        if (id != inspector.Code) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["InspectionCode"] = new SelectList(_context.Inspections, "Code", "Name", inspector.InspectionCode);
            return View(inspector);
        }

        try
        {
            _context.Update(inspector);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Inspectors.Any(e => e.Code == id))
                return NotFound();
            else
                throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Inspectors/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var inspector = await _context.Inspectors
            .Include(i => i.Inspection)
            .FirstOrDefaultAsync(i => i.Code == id);

        if (inspector == null) return NotFound();

        return View(inspector);
    }

    // POST: Inspectors/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var inspector = await _context.Inspectors.FindAsync(id);
        if (inspector != null)
        {
            _context.Inspectors.Remove(inspector);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
