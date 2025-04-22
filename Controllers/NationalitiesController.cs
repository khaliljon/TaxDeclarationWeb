using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireChiefInspector")]
public class NationalitiesController : Controller
{
    private readonly ApplicationDbContext _context;

    public NationalitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Nationalities
    public async Task<IActionResult> Index()
    {
        return View(await _context.Nationalities.ToListAsync());
    }

    // GET: Nationalities/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
            return NotFound();

        var nationality = await _context.Nationalities
            .FirstOrDefaultAsync(n => n.Code == id);
        if (nationality == null)
            return NotFound();

        return View(nationality);
    }

    // GET: Nationalities/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Nationalities/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Name")] Nationality nationality)
    {
        if (!ModelState.IsValid)
            return View(nationality);

        _context.Add(nationality);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Nationalities/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
            return NotFound();

        var nationality = await _context.Nationalities.FindAsync(id);
        if (nationality == null)
            return NotFound();

        return View(nationality);
    }

    // POST: Nationalities/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Code,Name")] Nationality nationality)
    {
        if (id != nationality.Code)
            return NotFound();

        if (!ModelState.IsValid)
            return View(nationality);

        try
        {
            _context.Update(nationality);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Nationalities.Any(n => n.Code == id))
                return NotFound();
            else
                throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Nationalities/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
            return NotFound();

        var nationality = await _context.Nationalities
            .FirstOrDefaultAsync(n => n.Code == id);
        if (nationality == null)
            return NotFound();

        return View(nationality);
    }

    // POST: Nationalities/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var nationality = await _context.Nationalities.FindAsync(id);
        if (nationality != null)
        {
            _context.Nationalities.Remove(nationality);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
