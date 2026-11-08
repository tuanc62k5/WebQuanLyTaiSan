using Microsoft.AspNetCore.Mvc;

namespace DoAn.Areas.Admin.controller;

[Area("Admin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}