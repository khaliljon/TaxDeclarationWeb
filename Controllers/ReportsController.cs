using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "ChiefInspector,Admin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Отчёт 1
    [HttpGet]
    public async Task<IActionResult> NerezidentsByInspection(int? inspectionCode)
    {
        ViewBag.Inspections = await _context.Inspections.OrderBy(i => i.Name).ToListAsync();
        if (inspectionCode == null)
            return View(new List<Taxpayer>());

        var list = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => t.InspectionCode == inspectionCode.Value &&
                        !t.IsResident &&
                        t.IsDeclarationRequired)
            .ToListAsync();

        return View(list);
    }


    // Отчёт 2
    [HttpGet]
    public async Task<IActionResult> CategoriesByAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return View(new List<Category>());

        var categoryCodes = await _context.Taxpayers
            .Where(t => EF.Functions.Like(t.Address, $"%{address}%"))
            .Select(t => t.CategoryCode)
            .Distinct()
            .ToListAsync();

        var result = await _context.Categories
            .Where(c => categoryCodes.Contains(c.Code))
            .ToListAsync();

        ViewBag.Address = address;
        return View(result);
    }

    // Отчёт 3
    [HttpGet]
    public async Task<IActionResult> DeclarationsCountByInspection()
    {
        var currentYear = DateTime.Now.Year;

        var allDeclarations = await _context.Declarations
            .Include(d => d.Inspection)
            .Where(d => d.Year == currentYear)
            .ToListAsync();

        var grouped = allDeclarations
            .GroupBy(d => d.Inspection?.Name ?? "Без названия")
            .Select(g => new
            {
                InspectionName = g.Key,
                Count = g.Count()
            })
            .ToList();

        return View(grouped);
    }

    // Отчёт 4
    [HttpGet]
    public async Task<IActionResult> DeclarationsByDateAndInspection(int? inspectionCode, int? day)
    {
        ViewBag.Inspections = await _context.Inspections.ToListAsync();

        if (inspectionCode == null || day == null)
            return View(new List<Declaration>());

        var list = await _context.Declarations
            .Include(d => d.Taxpayer)
            .Include(d => d.Inspection)
            .Include(d => d.Inspector)
            .Where(d => d.InspectionId == inspectionCode.Value && d.SubmittedAt.Day == day.Value)
            .ToListAsync();

        return View(list);
    }

    // Отчёт 5
    [HttpGet]
    public async Task<IActionResult> TaxableExpenses()
    {
        var total = await _context.Declarations
            .Select(d => d.Expenses - d.NonTaxableExpenses)
            .SumAsync();

        ViewBag.Total = total;
        return View();
    }

    // Отчёт 6
    [HttpGet]
    public async Task<IActionResult> MultipleIncomeCategory()
    {
        var taxpayers = await _context.Taxpayers
            .Include(t => t.Category)
            .Include(t => t.Inspection)
            .Where(t => t.Category.Name == "несколько мест получения дохода")
            .ToListAsync();

        return View(taxpayers);
    }

    // Отчёт 7
    [HttpGet]
    public async Task<IActionResult> DeclarationsSubmittedThisMonth()
    {
        var now = DateTime.Now;

        var declarations = await _context.Declarations
            .Include(d => d.Taxpayer)
            .Include(d => d.Inspection)
            .Include(d => d.Inspector)
            .Where(d => d.SubmittedAt.Month == now.Month && d.SubmittedAt.Year == now.Year)
            .ToListAsync();

        return View(declarations);
    }

    // Отчёт 8
    [HttpGet]
    public async Task<IActionResult> MaleTaxpayersOlderThan(int? age)
    {
        if (age is null || age < 0)
            return View(new List<Taxpayer>());

        var threshold = DateTime.Today.AddYears(-age.Value);

        var list = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => t.Gender == "М" && t.BirthDate < threshold)
            .ToListAsync();

        ViewBag.Age = age;
        return View(list);
    }

    // Отчёт 9
    [HttpGet]
    public async Task<IActionResult> TaxpayersBornInYear(int? year)
    {
        if (year is null || year < 1900)
            return View(new List<Taxpayer>());

        var list = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => t.BirthDate.Year == year)
            .ToListAsync();

        ViewBag.Year = year;
        return View(list);
    }

    // Отчёт 10
    [HttpGet]
    public async Task<IActionResult> TaxpayersByCategoryAndDate(int? categoryCode, int? day)
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();

        if (categoryCode == null || day == null)
            return View(new List<Taxpayer>());

        var list = await _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => t.CategoryCode == categoryCode.Value &&
                        _context.Declarations.Any(d => d.TaxpayerIIN == t.IIN && d.SubmittedAt.Day == day.Value))
            .ToListAsync();

        ViewBag.CategoryCode = categoryCode;
        ViewBag.Day = day;
        return View(list);
    }

    // Отчёт 11: вставка 3 стран
    [HttpGet]
    public IActionResult InsertCountries()
    {
        return View(new List<Country> { new(), new(), new() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InsertCountries(List<Country> countries)
    {
        var validCountries = countries
            .Where(c => c.Code > 0 && !string.IsNullOrWhiteSpace(c.Name))
            .ToList();

        if (validCountries.Any())
        {
            _context.Countries.AddRange(validCountries);
            await _context.SaveChangesAsync();
            ViewBag.Message = "Добавлено строк: " + validCountries.Count;
        }
        else
        {
            ViewBag.Message = "Ни одной корректной строки не введено.";
        }

        return View(new List<Country> { new(), new(), new() });
    }
    
    // Отчёт 12: количество налогоплательщиков по дню подачи декларации
    [HttpGet]
    public async Task<IActionResult> CountTaxpayersByDeclarationDay(int? day)
    {
        if (day is null || day < 1 || day > 31)
        {
            ViewBag.Result = null;
            return View();
        }

        var count = await _context.Declarations
            .Where(d => d.SubmittedAt.Day == day)
            .Select(d => d.TaxpayerIIN)
            .Distinct()
            .CountAsync();

        ViewBag.Day = day;
        ViewBag.Result = count;
        return View();
    }
}
