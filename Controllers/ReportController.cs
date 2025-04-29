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
        ViewBag.Address = address;

        List<dynamic> result = new();

        if (!string.IsNullOrWhiteSpace(address))
        {
            var rawData = await _context.Taxpayers
                .Where(t => EF.Functions.Like(t.Address, $"%{address}%"))
                .Join(_context.Categories,
                    taxpayer => taxpayer.CategoryCode,
                    category => category.Code,
                    (taxpayer, category) => new { taxpayer.Address, CategoryName = category.Name })
                .ToListAsync();

            result = rawData
                .GroupBy(tc => tc.Address)
                .Select(g => new
                {
                    Address = g.Key,
                    Categories = string.Join(", ", g.Select(tc => tc.CategoryName).Distinct())
                })
                .ToList<dynamic>();
        }

        ViewBag.CategoriesByAddress = result;
        return View();
    }

    // 3. Общее количество деклараций, поданных в каждую из инспекций в текущем году
    [HttpGet]
    public async Task<IActionResult> DeclarationsCountByInspection()
    {
        var currentYear = DateTime.Now.Year;

        var grouped = await _context.Declarations
            .Include(d => d.Inspection)
            .Where(d => d.Year == currentYear)
            .GroupBy(d => d.Inspection.Name)
            .Select(g => new
            {
                InspectionName = g.Key ?? "Неизвестная инспекция",
                DeclarationCount = g.Count()
            })
            .ToListAsync<dynamic>();

        ViewBag.DeclarationsCount = grouped;

        return View();
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
            .Join(_context.Taxpayers,
                d => d.TaxpayerIIN,
                t => t.IIN,
                (d, t) => new { Declaration = d, Taxpayer = t })
            .Where(x => x.Taxpayer.IsDeclarationRequired == true)
            .SumAsync(x => (decimal?)x.Declaration.Expenses);

        var finalTotal = total ?? 0;

        Console.WriteLine($"[DEBUG] Сумма расходов для налогообложения: {finalTotal} ₸");

        // Здесь возвращаем прямо decimal модель
        return View(model: finalTotal);
    }

    // 6. Список плательщиков, относящихся к категории с несколькими местами дохода
    [HttpGet]
    public async Task<IActionResult> MultipleIncomeCategory()
    {
        var taxpayers = await _context.Taxpayers
            .Include(t => t.Category)
            .Include(t => t.Nationality)
            .Include(t => t.Country)
            .Where(t => t.Category != null && t.Category.Name == "Имеет несколько мест получения дохода")
            .Select(t => new
            {
                t.IIN,
                t.FullName,
                t.Address,
                t.Phone,
                BirthDate = t.BirthDate.ToString("yyyy-MM-dd"),
                t.Gender,
                NationalityName = t.Nationality != null ? t.Nationality.Name : "—",
                Workplace = t.Workplace ?? "—",
                CountryName = t.Country != null ? t.Country.Name : "—",
                CategoryName = t.Category.Name
            })
            .ToListAsync<dynamic>();

        return View(model: taxpayers);
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
    public async Task<IActionResult> TaxpayersByDayAndInspection(int? day, int? inspectionId)
    {
        ViewBag.Inspections = await _context.Inspections.OrderBy(i => i.Name).ToListAsync();
        ViewBag.Day = day;
        ViewBag.InspectionId = inspectionId;

        List<dynamic> result = new();

        // Сброс некорректных значений
        if (day.HasValue && (day <= 0 || day > 31))
            day = null;

        if (inspectionId.HasValue && inspectionId <= 0)
            inspectionId = null;

        if (day.HasValue || inspectionId.HasValue)
        {
            var query = _context.Declarations
                .Include(d => d.Taxpayer)
                .Include(d => d.Inspection)
                .Include(d => d.Inspector)
                .AsQueryable();

            if (day.HasValue)
            {
                query = query.Where(d => d.SubmittedAt.Day == day.Value);
            }

            if (inspectionId.HasValue)
            {
                query = query.Where(d => d.InspectionId == inspectionId.Value);
            }

            result = await query
                .Select(d => new
                {
                    d.Taxpayer.IIN,
                    d.Taxpayer.FullName,
                    d.Taxpayer.Address,
                    d.Taxpayer.Phone,
                    InspectionName = d.Inspection.Name
                })
                .Distinct()
                .ToListAsync<dynamic>();
        }

        ViewBag.TaxpayersByDayAndInspection = result;

        return View();
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
