using BuildingCompanyManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildingCompanyManager.Controllers;

public class CompanyRegistrationController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        var model = new CompanyRegistrationInputModel();

        return View(model);
    }
}
