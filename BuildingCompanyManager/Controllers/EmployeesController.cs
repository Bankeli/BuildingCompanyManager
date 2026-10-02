using System.Security.Cryptography;
using BuildingCompanyManager.Common;
using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;
using BuildingCompanyManager.Models.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private const int TemporaryPasswordLength = 12;

    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public EmployeesController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var employees = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.CompanyId == owner.CompanyId)
            .OrderBy(employee => employee.Role)
            .ThenBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => new EmployeeListItemViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Role = employee.Role,
                JobTitle = employee.JobTitle,
                Email = dbContext.Users
                    .Where(user => user.Id == employee.UserId)
                    .Select(user => user.Email)
                    .FirstOrDefault() ?? string.Empty,
                CrewName = employee.Crew != null ? employee.Crew.Name : null,
                DailyRate = employee.DailyRate,
                IsActive = employee.IsActive
            })
            .ToListAsync();

        var model = new EmployeesIndexViewModel
        {
            CompanyName = owner.CompanyName,
            Employees = employees
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var model = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == id && employee.CompanyId == owner.CompanyId)
            .Select(employee => new EmployeeDetailsViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = dbContext.Users
                    .Where(user => user.Id == employee.UserId)
                    .Select(user => user.Email)
                    .FirstOrDefault() ?? string.Empty,
                PhoneNumber = employee.PhoneNumber,
                Role = employee.Role,
                JobTitle = employee.JobTitle,
                DailyRate = employee.DailyRate,
                IsActive = employee.IsActive,
                CrewId = employee.CrewId,
                CrewName = employee.Crew != null ? employee.Crew.Name : null,
                ForemanName = employee.Crew != null
                    ? employee.Crew.Foreman.FirstName + " " + employee.Crew.Foreman.LastName
                    : null,
                TechnicalManagerName = employee.Crew != null
                    ? employee.Crew.TechnicalManager.FirstName + " " + employee.Crew.TechnicalManager.LastName
                    : null
            })
            .SingleOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        if (model.CrewId is not null)
        {
            model.ActiveProjects = await dbContext.ProjectCrews
                .AsNoTracking()
                .Where(assignment =>
                    assignment.CrewId == model.CrewId &&
                    assignment.IsActive &&
                    assignment.Project.Status == ProjectStatus.Active)
                .GroupBy(assignment => new
                {
                    assignment.Project.Id,
                    assignment.Project.Name,
                    assignment.Project.ClientName
                })
                .Select(group => new EmployeeProjectItemViewModel
                {
                    Id = group.Key.Id,
                    Name = group.Key.Name,
                    ClientName = group.Key.ClientName
                })
                .OrderBy(project => project.Name)
                .ToListAsync();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        if (await GetCurrentOwnerAsync() is null)
        {
            return Forbid();
        }

        return View(new EmployeeCreationInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(EmployeeCreationInputModel model)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);

        if (model.Role is not EmployeeRole.TechnicalManager and not EmployeeRole.Foreman and not EmployeeRole.Worker)
        {
            ModelState.AddModelError(nameof(model.Role), EmployeeCreationErrorMessages.InvalidRole);
        }

        if (await userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), EmployeeCreationErrorMessages.EmailAlreadyExists);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var temporaryPassword = GenerateTemporaryPassword();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var identityResult = await userManager.CreateAsync(user, temporaryPassword);

            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await transaction.RollbackAsync();

                return View(model);
            }

            var employee = new Employee
            {
                UserId = user.Id,
                CompanyId = owner.CompanyId,
                Role = model.Role!.Value,
                JobTitle = model.JobTitle,
                DailyRate = model.DailyRate!.Value,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            dbContext.Employees.Add(employee);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, EmployeeCreationErrorMessages.EmployeeCreationFailed);

            return View(model);
        }

        TempData["SuccessMessage"] = "Employee account created successfully.";
        TempData["NewEmployeeEmail"] = model.Email;
        TempData["GeneratedPassword"] = temporaryPassword;

        return RedirectToAction(nameof(Index));
    }

    private async Task<OwnerCompanyContext?> GetCurrentOwnerAsync()
    {
        var userId = userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.UserId == userId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.Owner)
            .Select(employee => new OwnerCompanyContext(
                employee.CompanyId,
                employee.Company.Name))
            .SingleOrDefaultAsync();
    }

    private static string GenerateTemporaryPassword()
    {
        const string uppercaseLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercaseLetters = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@$%*?";
        const string allCharacters = uppercaseLetters + lowercaseLetters + digits + symbols;

        var password = new char[TemporaryPasswordLength];
        password[0] = GetRandomCharacter(uppercaseLetters);
        password[1] = GetRandomCharacter(lowercaseLetters);
        password[2] = GetRandomCharacter(digits);
        password[3] = GetRandomCharacter(symbols);

        for (var index = 4; index < password.Length; index++)
        {
            password[index] = GetRandomCharacter(allCharacters);
        }

        for (var index = password.Length - 1; index > 0; index--)
        {
            var randomIndex = RandomNumberGenerator.GetInt32(index + 1);
            (password[index], password[randomIndex]) = (password[randomIndex], password[index]);
        }

        return new string(password);
    }

    private static char GetRandomCharacter(string characters)
    {
        return characters[RandomNumberGenerator.GetInt32(characters.Length)];
    }

    private static void NormalizeInput(EmployeeCreationInputModel model)
    {
        model.FirstName = model.FirstName?.Trim() ?? string.Empty;
        model.LastName = model.LastName?.Trim() ?? string.Empty;
        model.Email = (model.Email?.Trim() ?? string.Empty).ToLowerInvariant();
        model.JobTitle = model.JobTitle?.Trim() ?? string.Empty;
    }

    private sealed record OwnerCompanyContext(int CompanyId, string CompanyName);
}
