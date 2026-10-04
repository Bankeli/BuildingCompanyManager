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
    private const int EmployeesPerPage = 10;

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
    public async Task<IActionResult> Index(int? crewId, int? projectId, int page = 1)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var selectedCrewId = await GetValidCrewIdAsync(crewId, owner.CompanyId);
        var selectedProjectId = await GetValidProjectIdAsync(projectId, owner.CompanyId);

        var employeesQuery = dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.CompanyId == owner.CompanyId);

        if (selectedCrewId is not null)
        {
            employeesQuery = employeesQuery.Where(employee =>
                employee.CrewId == selectedCrewId ||
                dbContext.Crews.Any(crew =>
                    crew.Id == selectedCrewId &&
                    (crew.ForemanId == employee.Id || crew.TechnicalManagerId == employee.Id)));
        }

        if (selectedProjectId is not null)
        {
            employeesQuery = employeesQuery.Where(employee =>
                dbContext.Projects.Any(project =>
                    project.Id == selectedProjectId &&
                    project.TechnicalManagerId == employee.Id) ||
                dbContext.ProjectCrews.Any(assignment =>
                    assignment.ProjectId == selectedProjectId &&
                    assignment.IsActive &&
                    (assignment.Crew.ForemanId == employee.Id ||
                     assignment.Crew.TechnicalManagerId == employee.Id ||
                     assignment.Crew.Members.Any(member => member.Id == employee.Id))));
        }

        var totalEmployeesCount = await employeesQuery.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalEmployeesCount / (double)EmployeesPerPage));
        var currentPage = Math.Clamp(page, 1, totalPages);

        var employees = await employeesQuery
            .OrderBy(employee => employee.Role)
            .ThenBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Skip((currentPage - 1) * EmployeesPerPage)
            .Take(EmployeesPerPage)
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
            Employees = employees,
            Crews = await dbContext.Crews
                .AsNoTracking()
                .Where(crew => crew.CompanyId == owner.CompanyId && crew.IsActive)
                .OrderBy(crew => crew.Name)
                .Select(crew => new EmployeeFilterOptionViewModel
                {
                    Id = crew.Id,
                    Name = crew.Name
                })
                .ToListAsync(),
            Projects = await dbContext.Projects
                .AsNoTracking()
                .Where(project => project.CompanyId == owner.CompanyId)
                .OrderBy(project => project.Name)
                .Select(project => new EmployeeFilterOptionViewModel
                {
                    Id = project.Id,
                    Name = project.Name
                })
                .ToListAsync(),
            SelectedCrewId = selectedCrewId,
            SelectedProjectId = selectedProjectId,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalEmployeesCount = totalEmployeesCount
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
                IsActive = employee.IsActive
            })
            .SingleOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        model.CrewAssignments = await dbContext.Crews
            .AsNoTracking()
            .Where(crew =>
                crew.CompanyId == owner.CompanyId &&
                crew.IsActive &&
                (crew.ForemanId == model.Id ||
                 crew.TechnicalManagerId == model.Id ||
                 crew.Members.Any(member => member.Id == model.Id)))
            .OrderBy(crew => crew.Name)
            .Select(crew => new EmployeeCrewAssignmentViewModel
            {
                Id = crew.Id,
                Name = crew.Name,
                ForemanName = crew.Foreman.FirstName + " " + crew.Foreman.LastName,
                TechnicalManagerName = crew.TechnicalManager.FirstName + " " + crew.TechnicalManager.LastName
            })
            .ToListAsync();

        var crewIds = model.CrewAssignments.Select(crew => crew.Id).ToArray();
        var projects = new Dictionary<int, EmployeeProjectItemViewModel>();

        var managedProjects = await dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.CompanyId == owner.CompanyId &&
                project.TechnicalManagerId == model.Id)
            .Select(project => new EmployeeProjectItemViewModel
            {
                Id = project.Id,
                Name = project.Name,
                ClientName = project.ClientName,
                Status = project.Status,
                IsTechnicalManager = true
            })
            .ToListAsync();

        foreach (var project in managedProjects)
        {
            projects[project.Id] = project;
        }

        if (crewIds.Length > 0)
        {
            var crewProjectRows = await dbContext.ProjectCrews
                .AsNoTracking()
                .Where(assignment =>
                    crewIds.Contains(assignment.CrewId) &&
                    assignment.IsActive)
                .Select(assignment => new CrewProjectRow(
                    assignment.Project.Id,
                    assignment.Project.Name,
                    assignment.Project.ClientName,
                    assignment.Project.Status,
                    assignment.Crew.Name))
                .ToListAsync();

            foreach (var projectGroup in crewProjectRows.GroupBy(project => new
                     {
                         project.Id,
                         project.Name,
                         project.ClientName,
                         project.Status
                     }))
            {
                var crewNames = projectGroup
                    .Select(project => project.CrewName)
                    .Distinct()
                    .OrderBy(crewName => crewName)
                    .ToArray();

                if (projects.TryGetValue(projectGroup.Key.Id, out var managedProject))
                {
                    managedProject.CrewNames = crewNames;
                    continue;
                }

                projects[projectGroup.Key.Id] = new EmployeeProjectItemViewModel
                {
                    Id = projectGroup.Key.Id,
                    Name = projectGroup.Key.Name,
                    ClientName = projectGroup.Key.ClientName,
                    Status = projectGroup.Key.Status,
                    CrewNames = crewNames
                };
            }
        }

        model.ProjectAssignments = projects.Values
            .OrderBy(project => project.Name)
            .ToArray();

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

    private async Task<int?> GetValidCrewIdAsync(int? crewId, int companyId)
    {
        if (crewId is null)
        {
            return null;
        }

        return await dbContext.Crews.AnyAsync(crew =>
            crew.Id == crewId &&
            crew.CompanyId == companyId &&
            crew.IsActive)
            ? crewId
            : null;
    }

    private async Task<int?> GetValidProjectIdAsync(int? projectId, int companyId)
    {
        if (projectId is null)
        {
            return null;
        }

        return await dbContext.Projects.AnyAsync(project =>
            project.Id == projectId &&
            project.CompanyId == companyId)
            ? projectId
            : null;
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

    private sealed record CrewProjectRow(
        int Id,
        string Name,
        string ClientName,
        ProjectStatus Status,
        string CrewName);

    private sealed record OwnerCompanyContext(int CompanyId, string CompanyName);
}
