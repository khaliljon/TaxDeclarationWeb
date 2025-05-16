using System.Dynamic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using TaxDeclarationWeb.Data;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "Inspector,ChiefInspector,Admin")]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int?> GetInspectorInspectionCode()
    {
        var user = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(user, "Inspector"))
        {
            var inspector = await _context.Inspectors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            return inspector?.InspectionCode;
        }
        return null;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> NerezidentsByInspection(int? inspectionId, string? inspectionName)
    {
        if (!User.IsInRole("ChiefInspector") && !User.IsInRole("Admin"))
        {
            inspectionId = await GetInspectorInspectionCode();
        }

        ViewBag.InspectionId = inspectionId;
        ViewBag.InspectionName = inspectionName;

        var query = _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => !t.IsResident && t.IsDeclarationRequired);

        if (inspectionId.HasValue)
            query = query.Where(t => t.InspectionCode == inspectionId.Value);
        else if (!string.IsNullOrWhiteSpace(inspectionName))
            query = query.Where(t => t.Inspection.Name.Contains(inspectionName));

        var result = await query.Select(t => new
        {
            t.IIN,
            t.FullName,
            t.Address,
            t.Phone,
            InspectionName = t.Inspection.Name
        }).ToListAsync<dynamic>();

        ViewBag.NerezidentsByInspection = result;
        return View();
    }

    public async Task<IActionResult> CategoriesByAddress(string? address)
    {
        ViewBag.Address = address;
        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Taxpayers.AsQueryable();
        if (inspectionCode.HasValue)
            query = query.Where(t => t.InspectionCode == inspectionCode.Value);

        if (!string.IsNullOrWhiteSpace(address))
        {
            var rawData = await query
                .Where(t => EF.Functions.Like(t.Address, $"%{address}%"))
                .Join(_context.Categories,
                      t => t.CategoryCode,
                      c => c.Code,
                      (t, c) => new { t.Address, CategoryName = c.Name })
                .ToListAsync();

            var result = rawData.GroupBy(tc => tc.Address)
                .Select(g => new
                {
                    Address = g.Key,
                    Categories = string.Join(", ", g.Select(tc => tc.CategoryName).Distinct())
                })
                .ToList<dynamic>();

            ViewBag.CategoriesByAddress = result;
        }

        return View();
    }

    public async Task<IActionResult> DeclarationsCountByInspection()
    {
        var currentYear = DateTime.Now.Year;
        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Declarations
            .Include(d => d.Inspection)
            .Where(d => d.Year == currentYear);

        if (inspectionCode.HasValue)
            query = query.Where(d => d.InspectionId == inspectionCode.Value);

        var grouped = await query.GroupBy(d => d.Inspection.Name)
            .Select(g => new
            {
                InspectionName = g.Key ?? "Неизвестная инспекция",
                DeclarationCount = g.Count()
            }).ToListAsync<dynamic>();

        ViewBag.DeclarationsCount = grouped;
        return View();
    }

    public async Task<IActionResult> TaxpayersByDayAndInspection(int? inspectionId, int? day)
    {
        if (!User.IsInRole("ChiefInspector") && !User.IsInRole("Admin"))
            inspectionId = await GetInspectorInspectionCode();

        ViewBag.InspectionId = inspectionId;
        ViewBag.Day = day;

        if (inspectionId == null || day == null)
            return View(new List<dynamic>());

        var list = await _context.Declarations
            .Include(d => d.Taxpayer)
            .Where(d => d.InspectionId == inspectionId.Value && d.SubmittedAt.Day == day.Value)
            .Select(d => new
            {
                d.Taxpayer.IIN,
                d.Taxpayer.FullName,
                d.Taxpayer.Address,
                d.Taxpayer.Phone
            }).Cast<dynamic>()
            .ToListAsync();

        ViewBag.TaxpayersByDayAndInspection = list;
        return View();
    }

    public async Task<IActionResult> TaxableExpenses()
    {
        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Declarations
            .Join(_context.Taxpayers,
                  d => d.TaxpayerIIN,
                  t => t.IIN,
                  (d, t) => new { Declaration = d, Taxpayer = t })
            .Where(x => x.Taxpayer.IsDeclarationRequired);

        if (inspectionCode.HasValue)
            query = query.Where(x => x.Declaration.InspectionId == inspectionCode.Value);

        var total = await query.SumAsync(x => (decimal?)x.Declaration.Expenses) ?? 0;
        return View(model: total);
    }

    public async Task<IActionResult> MultipleIncomeCategory()
    {
        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Taxpayers
            .Include(t => t.Category)
            .Include(t => t.Nationality)
            .Include(t => t.Country)
            .Where(t => t.Category != null && t.Category.Name == "Имеет несколько мест получения дохода");

        if (inspectionCode.HasValue)
            query = query.Where(t => t.InspectionCode == inspectionCode.Value);

        var result = await query.Select(t => new
        {
            t.IIN,
            t.FullName,
            t.Address,
            t.Phone,
            BirthDate = t.BirthDate.ToString("yyyy-MM-dd"),
            t.Gender,
            NationalityName = t.Nationality.Name ?? "—",
            Workplace = t.Workplace ?? "—",
            CountryName = t.Country.Name ?? "—",
            CategoryName = t.Category.Name
        }).ToListAsync<dynamic>();

        return View(model: result);
    }

    public async Task<IActionResult> DeclarationsSubmittedThisMonth()
    {
        var now = DateTime.Now;
        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Declarations
            .Include(d => d.Taxpayer)
            .Include(d => d.Inspection)
            .Include(d => d.Inspector)
            .Where(d => d.SubmittedAt.Month == now.Month && d.SubmittedAt.Year == now.Year);

        if (inspectionCode.HasValue)
            query = query.Where(d => d.InspectionId == inspectionCode.Value);

        var result = await query.Select(d => new
        {
            DeclarationId = d.Id, 
            InspectionName = d.Inspection.Name,
            InspectorName = d.Inspector.FullName,
            d.SubmittedAt,
            d.Year,
            TaxpayerName = d.Taxpayer.FullName,
            d.Income,
            d.Expenses,
            d.NonTaxableExpenses,
            d.PaidTaxes,
            Profit = d.Income - d.Expenses
        }).ToListAsync<dynamic>();
        return View(result);
    }

    public async Task<IActionResult> MaleTaxpayersOlderThan(int? age)
    {
        ViewBag.Age = age;
        if (age == null || age < 0)
            return View(new List<Taxpayer>());

        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => t.Gender == "М" && EF.Functions.DateDiffYear(t.BirthDate, DateTime.Today) > age);

        if (inspectionCode.HasValue)
            query = query.Where(t => t.InspectionCode == inspectionCode.Value);

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> TaxpayersBornInYear(int? year)
    {
        ViewBag.Year = year;
        if (year == null || year < 1900 || year > DateTime.Now.Year)
            return View(new List<Taxpayer>());

        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Taxpayers
            .Include(t => t.Inspection)
            .Where(t => t.BirthDate.Year == year);

        if (inspectionCode.HasValue)
            query = query.Where(t => t.InspectionCode == inspectionCode.Value);

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> TaxpayersByCategoryAndDate(int? categoryCode, int? day)
    {
        ViewBag.CategoryCode = categoryCode;
        ViewBag.Day = day;

        if (categoryCode == null || day == null)
            return View(new List<dynamic>());

        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Declarations
            .Include(d => d.Taxpayer)
            .ThenInclude(t => t.Category)
            .Where(d => d.SubmittedAt.Day == day && d.Taxpayer.CategoryCode == categoryCode);

        if (inspectionCode.HasValue)
            query = query.Where(d => d.InspectionId == inspectionCode.Value);

        var result = await query.Select(d => new
        {
            d.Taxpayer.IIN,
            d.Taxpayer.FullName,
            d.Taxpayer.Address,
            d.Taxpayer.Phone,
            CategoryName = d.Taxpayer.Category.Name
        }).Cast<dynamic>().ToListAsync();

        ViewBag.TaxpayersByCategoryAndDate = result;
        return View();
    }

    public async Task<IActionResult> CountTaxpayersByDeclarationDay(int? day)
    {
        if (day is null || day < 1 || day > 31)
        {
            ViewBag.Result = null;
            return View();
        }

        var inspectionCode = await GetInspectorInspectionCode();

        var query = _context.Declarations
            .Where(d => d.SubmittedAt.Day == day);

        if (inspectionCode.HasValue)
            query = query.Where(d => d.InspectionId == inspectionCode.Value);

        var count = await query.Select(d => d.TaxpayerIIN).Distinct().CountAsync();

        ViewBag.Day = day;
        ViewBag.Result = count;
        return View();
    }

    [HttpPost]
    public IActionResult ExportToPdf(string viewName, string jsonModel)
    {
        if (string.IsNullOrWhiteSpace(jsonModel))
            return NotFound("Нет модели");

        var model = JsonConvert.DeserializeObject<List<object>>(jsonModel);
        return new ViewAsPdf(viewName, model)
        {
            FileName = $"{viewName}_{DateTime.Now:yyyyMMddHHmmss}.pdf",
            PageSize = Size.A4
        };
    }
}
