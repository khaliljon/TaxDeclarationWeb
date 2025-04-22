using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "Inspector,ChiefInspector,Admin")]
[ApiController]
[Route("taxpayers")]
public class TaxpayersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaxpayersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET /taxpayers
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        IQueryable<Taxpayer> query = _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Country)
            .Include(t => t.Nationality);

        if (roles.Contains("Inspector") && user.InspectorId != null)
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.Code == user.InspectorId);
            if (inspector != null)
            {
                query = query.Where(t => t.InspectionCode == inspector.InspectionCode);
            }
        }

        var result = await query.ToListAsync();
        return Ok(result);
    }

    // GET /taxpayers/{iin}
    [HttpGet("{iin}")]
    public async Task<IActionResult> Details(string iin)
    {
        var taxpayer = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Include(t => t.Category)
            .Include(t => t.Country)
            .Include(t => t.Nationality)
            .FirstOrDefaultAsync(t => t.IIN == iin);

        if (taxpayer == null)
            return NotFound();

        return Ok(taxpayer);
    }

    // POST /taxpayers
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Taxpayer taxpayer)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Taxpayers.Add(taxpayer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Details), new { iin = taxpayer.IIN }, taxpayer);
    }

    // PUT /taxpayers/{iin}
    [HttpPut("{iin}")]
    public async Task<IActionResult> Edit(string iin, [FromBody] Taxpayer updated)
    {
        if (iin != updated.IIN)
            return BadRequest("IIN mismatch");

        var existing = await _context.Taxpayers.FindAsync(iin);
        if (existing == null)
            return NotFound();

        _context.Entry(existing).CurrentValues.SetValues(updated);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /taxpayers/{iin}
    [HttpDelete("{iin}")]
    public async Task<IActionResult> Delete(string iin)
    {
        var taxpayer = await _context.Taxpayers.FindAsync(iin);
        if (taxpayer == null)
            return NotFound();

        _context.Taxpayers.Remove(taxpayer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
