using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaxDeclarationWeb.Controllers;

[Authorize(Roles = "Inspector")]
public class InspectorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}