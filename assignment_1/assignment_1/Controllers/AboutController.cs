using Microsoft.AspNetCore.Mvc;

namespace Christopher_Sanden.Controllers;

public class AboutController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}