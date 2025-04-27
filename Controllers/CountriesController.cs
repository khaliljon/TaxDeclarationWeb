using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireChiefInspector")]
public class CountriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CountriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Countries
    public async Task<IActionResult> Index()
    {
        return View(await _context.Countries.ToListAsync());
    }

    // GET: Countries/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Code == id);
        if (country == null)
            return NotFound();

        return View(country);
    }

    // GET: Countries/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Countries/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Name")] Country country)
    {
        if (!ModelState.IsValid)
            return View(country);

        _context.Add(country);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Countries/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        return View(country);
    }

    // POST: Countries/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Code,Name")] Country country)
    {
        if (id != country.Code)
            return NotFound();

        if (!ModelState.IsValid)
            return View(country);

        try
        {
            _context.Update(country);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Countries.Any(e => e.Code == id))
                return NotFound();
            else
                throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Countries/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Code == id);
        if (country == null)
            return NotFound();

        return View(country);
    }

    // POST: Countries/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country != null)
        {
            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
