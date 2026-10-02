using BuildingCompanyManager.Common;
using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;
using BuildingCompanyManager.Models.Crews;
using BuildingCompanyManager.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class CrewsController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public CrewsController(
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

        var crews = await dbContext.Crews
            .AsNoTracking()
            .Where(crew => crew.CompanyId == owner.CompanyId)
            .OrderByDescending(crew => crew.IsActive)
            .ThenBy(crew => crew.Name)
            .Select(crew => new CrewListItemViewModel
            {
                Id = crew.Id,
                Name = crew.Name,
                TechnicalManagerName = crew.TechnicalManager.FirstName + " " + crew.TechnicalManager.LastName,
                ForemanName = crew.Foreman.FirstName + " " + crew.Foreman.LastName,
                WorkersCount = crew.Members.Count(member => member.IsActive),
                IsActive = crew.IsActive
            })
            .ToListAsync();

        return View(new CrewIndexViewModel
        {
            CompanyName = owner.CompanyName,
            Crews = crews
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var model = new CrewFormViewModel();
        await PopulateFormOptionsAsync(model, owner.CompanyId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrewFormViewModel model)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);

        await ValidateCrewAsync(model, owner.CompanyId);

        if (!ModelState.IsValid)
        {
            await PopulateFormOptionsAsync(model, owner.CompanyId);

            return View(model);
        }

        var crew = new Crew
        {
            CompanyId = owner.CompanyId,
            Name = model.Name,
            TechnicalManagerId = model.TechnicalManagerId!.Value,
            ForemanId = model.ForemanId!.Value
        };

        var workers = await GetSelectedWorkersAsync(model.WorkerIds, owner.CompanyId, null);
        dbContext.Crews.Add(crew);

        foreach (var worker in workers)
        {
            worker.Crew = crew;
        }

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, CrewErrorMessages.CrewUpdateFailed);
            await PopulateFormOptionsAsync(model, owner.CompanyId);

            return View(model);
        }

        TempData["SuccessMessage"] = "Crew created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var crew = await dbContext.Crews
            .AsNoTracking()
            .Where(item => item.Id == id && item.CompanyId == owner.CompanyId && item.IsActive)
            .Select(item => new CrewFormViewModel
            {
                Id = item.Id,
                Name = item.Name,
                TechnicalManagerId = item.TechnicalManagerId,
                ForemanId = item.ForemanId,
                WorkerIds = item.Members.Select(member => member.Id).ToList()
            })
            .SingleOrDefaultAsync();

        if (crew is null)
        {
            return NotFound();
        }

        await PopulateFormOptionsAsync(crew, owner.CompanyId);

        return View(crew);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CrewFormViewModel model)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var crew = await dbContext.Crews
            .SingleOrDefaultAsync(item => item.Id == model.Id && item.CompanyId == owner.CompanyId && item.IsActive);

        if (crew is null)
        {
            return NotFound();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);

        await ValidateCrewAsync(model, owner.CompanyId);

        if (!ModelState.IsValid)
        {
            await PopulateFormOptionsAsync(model, owner.CompanyId);

            return View(model);
        }

        var workers = await GetSelectedWorkersAsync(model.WorkerIds, owner.CompanyId, crew.Id);
        var currentWorkers = await dbContext.Employees
            .Where(employee => employee.CrewId == crew.Id)
            .ToListAsync();

        crew.Name = model.Name;
        crew.TechnicalManagerId = model.TechnicalManagerId!.Value;
        crew.ForemanId = model.ForemanId!.Value;

        foreach (var worker in currentWorkers)
        {
            worker.CrewId = null;
        }

        foreach (var worker in workers)
        {
            worker.CrewId = crew.Id;
        }

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, CrewErrorMessages.CrewUpdateFailed);
            await PopulateFormOptionsAsync(model, owner.CompanyId);

            return View(model);
        }

        TempData["SuccessMessage"] = "Crew updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var crew = await dbContext.Crews
            .SingleOrDefaultAsync(item => item.Id == id && item.CompanyId == owner.CompanyId && item.IsActive);

        if (crew is null)
        {
            return NotFound();
        }

        if (await dbContext.ProjectCrews.AnyAsync(assignment => assignment.CrewId == crew.Id && assignment.IsActive))
        {
            TempData["ErrorMessage"] = CrewErrorMessages.CrewHasActiveAssignments;

            return RedirectToAction(nameof(Index));
        }

        var workers = await dbContext.Employees
            .Where(employee => employee.CrewId == crew.Id)
            .ToListAsync();

        crew.IsActive = false;

        foreach (var worker in workers)
        {
            worker.CrewId = null;
        }

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Crew deactivated successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCrewAsync(CrewFormViewModel model, int companyId)
    {
        if (model.TechnicalManagerId is null || !await dbContext.Employees.AnyAsync(employee =>
                employee.Id == model.TechnicalManagerId &&
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.TechnicalManager))
        {
            ModelState.AddModelError(nameof(model.TechnicalManagerId), CrewErrorMessages.InvalidTechnicalManager);
        }

        if (model.ForemanId is null || !await dbContext.Employees.AnyAsync(employee =>
                employee.Id == model.ForemanId &&
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.Foreman &&
                !dbContext.Crews.Any(crew => crew.Id != model.Id && crew.ForemanId == employee.Id && crew.IsActive)))
        {
            ModelState.AddModelError(nameof(model.ForemanId), CrewErrorMessages.InvalidForeman);
        }

        if (await dbContext.Crews.AnyAsync(crew =>
                crew.CompanyId == companyId &&
                crew.Id != model.Id &&
                crew.IsActive &&
                crew.Name == model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), CrewErrorMessages.CrewNameAlreadyExists);
        }

        var workerIds = model.WorkerIds.Distinct().ToList();
        var workersCount = await dbContext.Employees.CountAsync(employee =>
            workerIds.Contains(employee.Id) &&
            employee.CompanyId == companyId &&
            employee.IsActive &&
            employee.Role == EmployeeRole.Worker &&
            (employee.CrewId == null || employee.CrewId == model.Id));

        if (workersCount != workerIds.Count)
        {
            ModelState.AddModelError(nameof(model.WorkerIds), CrewErrorMessages.InvalidWorkers);
        }
    }

    private async Task<List<Employee>> GetSelectedWorkersAsync(
        IEnumerable<int> workerIds,
        int companyId,
        int? crewId)
    {
        var selectedIds = workerIds.Distinct().ToList();

        return await dbContext.Employees
            .Where(employee =>
                selectedIds.Contains(employee.Id) &&
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.Worker &&
                (employee.CrewId == null || employee.CrewId == crewId))
            .ToListAsync();
    }

    private async Task PopulateFormOptionsAsync(CrewFormViewModel model, int companyId)
    {
        model.TechnicalManagers = await GetEmployeeOptionsAsync(companyId, EmployeeRole.TechnicalManager);
        model.Foremen = await dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.Foreman &&
                !dbContext.Crews.Any(crew => crew.Id != model.Id && crew.ForemanId == employee.Id && crew.IsActive))
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .Select(employee => new EmployeeOptionViewModel
            {
                Id = employee.Id,
                FullName = employee.FirstName + " " + employee.LastName
            })
            .ToListAsync();
        model.Workers = await dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.Worker &&
                (employee.CrewId == null || employee.CrewId == model.Id))
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .Select(employee => new EmployeeOptionViewModel
            {
                Id = employee.Id,
                FullName = employee.FirstName + " " + employee.LastName
            })
            .ToListAsync();
    }

    private async Task<List<EmployeeOptionViewModel>> GetEmployeeOptionsAsync(int companyId, EmployeeRole role)
    {
        return await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.CompanyId == companyId && employee.IsActive && employee.Role == role)
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .Select(employee => new EmployeeOptionViewModel
            {
                Id = employee.Id,
                FullName = employee.FirstName + " " + employee.LastName
            })
            .ToListAsync();
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
            .Select(employee => new OwnerCompanyContext(employee.CompanyId, employee.Company.Name))
            .SingleOrDefaultAsync();
    }

    private static void NormalizeInput(CrewFormViewModel model)
    {
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.WorkerIds = model.WorkerIds.Distinct().ToList();
    }

    private sealed record OwnerCompanyContext(int CompanyId, string CompanyName);
}
