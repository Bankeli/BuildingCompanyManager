using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Common;
using BuildingCompanyManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

public class CompanyRegistrationController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public CompanyRegistrationController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        var model = new CompanyRegistrationInputModel();

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CompanyRegistrationInputModel model)
    {
        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await dbContext.Companies.AnyAsync(company => company.Name == model.CompanyName))
        {
            ModelState.AddModelError(
                nameof(model.CompanyName),
                CompanyRegistrationTexts.CompanyNameAlreadyExists);
        }

        if (await dbContext.Companies.AnyAsync(company =>
                company.RegistrationNumber == model.RegistrationNumber))
        {
            ModelState.AddModelError(
                nameof(model.RegistrationNumber),
                CompanyRegistrationTexts.RegistrationNumberAlreadyExists);
        }

        if (await userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), CompanyRegistrationTexts.EmailAlreadyExists);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var identityResult = await userManager.CreateAsync(user, model.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await transaction.RollbackAsync();

                return View(model);
            }

            var company = new Company
            {
                Name = model.CompanyName,
                RegistrationNumber = model.RegistrationNumber,
                Address = model.Address
            };

            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();

            var owner = new Employee
            {
                UserId = user.Id,
                CompanyId = company.Id,
                Role = EmployeeRole.Owner,
                JobTitle = EmployeeDefaults.OwnerJobTitle,
                DailyRate = null,
                FirstName = model.OwnerFirstName,
                LastName = model.OwnerLastName
            };

            dbContext.Employees.Add(owner);
            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            await signInManager.SignInAsync(user, isPersistent: false);

            TempData[RouteConstants.CompanyRegistrationSuccessMessageKey] = CompanyRegistrationTexts.RegistrationSucceeded;

            return RedirectToAction(RouteConstants.IndexAction, RouteConstants.HomeController);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(
                string.Empty,
                CompanyRegistrationTexts.RegistrationFailed);

            return View(model);
        }
    }

    private static void NormalizeInput(CompanyRegistrationInputModel model)
    {
        model.CompanyName = model.CompanyName?.Trim() ?? string.Empty;
        model.RegistrationNumber = model.RegistrationNumber?.Trim() ?? string.Empty;
        model.Address = model.Address?.Trim() ?? string.Empty;
        model.OwnerFirstName = model.OwnerFirstName?.Trim() ?? string.Empty;
        model.OwnerLastName = model.OwnerLastName?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
    }
}
