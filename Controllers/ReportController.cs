using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "ChiefInspector,Admin")]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    // === ГЛАВНАЯ СТРАНИЦА ОТЧЁТОВ ===
    [HttpGet]
    public IActionResult Index()
    {
        // Здесь можно подготовить какие-то общие данные для Index.cshtml если нужно
        return View();
    }

    // 1. Список нерезидентов по инспекции с обязательной подачей декларации
    [HttpGet]
    public async Task<IActionResult> NerezidentsByInspection(int? inspectionId, string? inspectionName)
    {
        ViewBag.InspectionId = inspectionId;
        ViewBag.InspectionName = inspectionName;

        List<dynamic> result = new();

        if (inspectionId.HasValue || !string.IsNullOrWhiteSpace(inspectionName))
        {
            var query = _context.Taxpayers
                .Include(t => t.Inspection)
                .Where(t => !t.IsResident && t.IsDeclarationRequired);

            if (inspectionId.HasValue)
            {
                query = query.Where(t => t.InspectionCode == inspectionId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(inspectionName))
            {
                query = query.Where(t => t.Inspection.Name.Contains(inspectionName));
            }

            result = await query
                .Select(t => new
                {
                    t.IIN,
                    t.FullName,
                    t.Address,
                    t.Phone,
                    InspectionName = t.Inspection.Name
                })
                .ToListAsync<dynamic>();
        }

        ViewBag.NerezidentsByInspection = result;

        return View();
    }
    
    // 2. Категории налогоплательщиков по адресу
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

    // 3. Общее количество деклараций, поданных в каждую из инспекций в текущем году
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

    // 4. Список, подавших декларацию i-го числа в j-ой инспекции
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

    // 5. Сумма расходов налогоплательщиков, которые должны облагаться налогом
    [HttpGet]
    public async Task<IActionResult> TaxableExpenses()
    {
        var total = await _context.Declarations
            .Select(d => d.Expenses - d.NonTaxableExpenses)
            .SumAsync();

        ViewBag.Total = total;
        return View();
    }

    // 6. Список плательщиков, относящихся к категории с несколькими местами дохода
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

    // 7. Декларации, поданные в текущем месяце
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

    // 8. Список мужчин-налогоплательщиков старше ... лет
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

    // 9. Налогоплательщики i-го года рождения
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

    // 10. Налогоплательщики i-й категории, подавшие декларацию j-го числа
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

    // 11. Вставка 3 новых строк в таблицу стран
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

    // 12. Количество налогоплательщиков, подавших декларацию в указанный день месяца
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
