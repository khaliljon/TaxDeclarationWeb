using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Policy = "RequireInspector")]
[ApiController]
[Route("declarations")]
public class DeclarationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeclarationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET /declarations
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        IQueryable<Declaration> query = _context.Declarations
            .Include(d => d.Taxpayer)
            .Include(d => d.Inspection)
            .Include(d => d.Inspector);

        if (roles.Contains("Inspector") && user.InspectorId != null)
        {
            var inspector = await _context.Inspectors
                .FirstOrDefaultAsync(i => i.Code == user.InspectorId);

            if (inspector != null)
            {
                query = query.Where(d => d.InspectionId == inspector.InspectionCode);
            }
        }

        var list = await query.ToListAsync();
        return Ok(list);
    }

    // GET /declarations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var declaration = await _context.Declarations
            .Include(d => d.Taxpayer)
            .Include(d => d.Inspection)
            .Include(d => d.Inspector)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (declaration == null)
            return NotFound();

        return Ok(declaration);
    }

    // POST /declarations
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Declaration declaration)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Declarations.Add(declaration);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = declaration.Id }, declaration);
    }

    // PUT /declarations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Declaration declaration)
    {
        if (id != declaration.Id)
            return BadRequest("ID mismatch");

        var existing = await _context.Declarations.FindAsync(id);
        if (existing == null)
            return NotFound();

        _context.Entry(existing).CurrentValues.SetValues(declaration);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /declarations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var declaration = await _context.Declarations.FindAsync(id);
        if (declaration == null)
            return NotFound();

        _context.Declarations.Remove(declaration);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
