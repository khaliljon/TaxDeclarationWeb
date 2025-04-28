using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaxDeclarationWeb.Models;

namespace TaxDeclarationWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Если пользователь авторизован — перенаправить по роли на нужный дашборд
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole("ChiefInspector"))
                return RedirectToAction("Index", "ChiefInspector");
            if (User.IsInRole("Inspector"))
                return RedirectToAction("Index", "InspectorRole");
            if (User.IsInRole("Taxpayer"))
                return RedirectToAction("Index", "Taxpayer");
        }

        // Иначе — просто главная страница для неавторизованных
        return View();
    }

    public IActionResult Privacy()
    {
        // Шаблонная страница политики конфиденциальности
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Красивая страница ошибки с идентификатором запроса
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}