using BuildingCompanyManager.Common;
using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;
using BuildingCompanyManager.Models.Projects;
using BuildingCompanyManager.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public ProjectsController(
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

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.CompanyId == owner.CompanyId)
            .OrderByDescending(project => project.Status == ProjectStatus.Active)
            .ThenByDescending(project => project.StartDate)
            .Select(project => new ProjectListItemViewModel
            {
                Id = project.Id,
                Name = project.Name,
                ClientName = project.ClientName,
                Status = project.Status,
                TechnicalManagerName = project.TechnicalManager.FirstName + " " + project.TechnicalManager.LastName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ActiveCrewsCount = project.ProjectCrews.Count(assignment => assignment.IsActive)
            })
            .ToListAsync();

        return View(new ProjectIndexViewModel
        {
            CompanyName = owner.CompanyName,
            Projects = projects
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

        var model = new ProjectFormViewModel();
        await PopulateTechnicalManagersAsync(model, owner.CompanyId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormViewModel model)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);
        await ValidateProjectAsync(model, owner.CompanyId);

        if (!ModelState.IsValid)
        {
            await PopulateTechnicalManagersAsync(model, owner.CompanyId);

            return View(model);
        }

        var project = new Project
        {
            CompanyId = owner.CompanyId,
            Name = model.Name,
            ClientName = model.ClientName,
            Address = model.Address,
            TechnicalManagerId = model.TechnicalManagerId!.Value,
            Status = model.Status!.Value,
            StartDate = model.StartDate!.Value,
            EndDate = model.EndDate
        };

        dbContext.Projects.Add(project);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, ProjectErrorMessages.ProjectUpdateFailed);
            await PopulateTechnicalManagersAsync(model, owner.CompanyId);

            return View(model);
        }

        TempData["SuccessMessage"] = "Project created successfully.";

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

        var model = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.Id == id && project.CompanyId == owner.CompanyId)
            .Select(project => new ProjectFormViewModel
            {
                Id = project.Id,
                Name = project.Name,
                ClientName = project.ClientName,
                Address = project.Address,
                TechnicalManagerId = project.TechnicalManagerId,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            })
            .SingleOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        await PopulateTechnicalManagersAsync(model, owner.CompanyId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectFormViewModel model)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var project = await dbContext.Projects
            .SingleOrDefaultAsync(item => item.Id == model.Id && item.CompanyId == owner.CompanyId);

        if (project is null)
        {
            return NotFound();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);
        await ValidateProjectAsync(model, owner.CompanyId);

        if (!ModelState.IsValid)
        {
            await PopulateTechnicalManagersAsync(model, owner.CompanyId);

            return View(model);
        }

        project.Name = model.Name;
        project.ClientName = model.ClientName;
        project.Address = model.Address;
        project.TechnicalManagerId = model.TechnicalManagerId!.Value;
        project.Status = model.Status!.Value;
        project.StartDate = model.StartDate!.Value;
        project.EndDate = model.EndDate;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, ProjectErrorMessages.ProjectUpdateFailed);
            await PopulateTechnicalManagersAsync(model, owner.CompanyId);

            return View(model);
        }

        TempData["SuccessMessage"] = "Project updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ManageCrews(int id)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var model = await BuildCrewManagementModelAsync(id, owner.CompanyId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCrew(ProjectCrewManagementViewModel model)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var assignmentModel = model.Assignment;
        var project = await dbContext.Projects
            .SingleOrDefaultAsync(item => item.Id == assignmentModel.ProjectId && item.CompanyId == owner.CompanyId);

        if (project is null)
        {
            return NotFound();
        }

        ModelState.Clear();
        TryValidateModel(assignmentModel, nameof(model.Assignment));
        await ValidateAssignmentAsync(assignmentModel, project, owner.CompanyId);

        if (!ModelState.IsValid)
        {
            var invalidModel = await BuildCrewManagementModelAsync(assignmentModel.ProjectId, owner.CompanyId, assignmentModel);

            return View(nameof(ManageCrews), invalidModel);
        }

        var assignment = new ProjectCrew
        {
            ProjectId = project.Id,
            CrewId = assignmentModel.CrewId!.Value,
            AssignedFrom = assignmentModel.AssignedFrom!.Value,
            AssignedTo = assignmentModel.AssignedTo
        };

        dbContext.ProjectCrews.Add(assignment);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, ProjectErrorMessages.ProjectUpdateFailed);
            var invalidModel = await BuildCrewManagementModelAsync(assignmentModel.ProjectId, owner.CompanyId, assignmentModel);

            return View(nameof(ManageCrews), invalidModel);
        }

        TempData["SuccessMessage"] = "Crew assigned to the project successfully.";

        return RedirectToAction(nameof(ManageCrews), new { id = project.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndCrewAssignment(int id)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var assignment = await dbContext.ProjectCrews
            .Include(item => item.Project)
            .SingleOrDefaultAsync(item =>
                item.Id == id &&
                item.IsActive &&
                item.Project.CompanyId == owner.CompanyId);

        if (assignment is null)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        assignment.IsActive = false;

        if (assignment.AssignedTo is null && assignment.AssignedFrom <= today)
        {
            assignment.AssignedTo = today;
        }

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Crew assignment ended successfully.";

        return RedirectToAction(nameof(ManageCrews), new { id = assignment.ProjectId });
    }

    private async Task ValidateProjectAsync(ProjectFormViewModel model, int companyId)
    {
        if (model.TechnicalManagerId is null || !await dbContext.Employees.AnyAsync(employee =>
                employee.Id == model.TechnicalManagerId &&
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.TechnicalManager))
        {
            ModelState.AddModelError(nameof(model.TechnicalManagerId), ProjectErrorMessages.InvalidTechnicalManager);
        }

        if (model.Status is not ProjectStatus.Planning and not ProjectStatus.Active and not ProjectStatus.OnHold and not ProjectStatus.Completed and not ProjectStatus.Cancelled)
        {
            ModelState.AddModelError(nameof(model.Status), ProjectErrorMessages.InvalidProjectStatus);
        }

        if (model.StartDate is not null && model.EndDate is not null && model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), ProjectErrorMessages.InvalidProjectDates);
        }

        if (await dbContext.Projects.AnyAsync(project =>
                project.CompanyId == companyId &&
                project.Id != model.Id &&
                project.Name == model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), ProjectErrorMessages.ProjectNameAlreadyExists);
        }
    }

    private async Task ValidateAssignmentAsync(
        ProjectCrewAssignmentInputModel model,
        Project project,
        int companyId)
    {
        if (model.CrewId is null || !await dbContext.Crews.AnyAsync(crew =>
                crew.Id == model.CrewId &&
                crew.CompanyId == companyId &&
                crew.IsActive))
        {
            ModelState.AddModelError(
                $"{nameof(ProjectCrewManagementViewModel.Assignment)}.{nameof(model.CrewId)}",
                ProjectErrorMessages.InvalidCrew);
        }

        if (model.AssignedFrom is not null && model.AssignedTo is not null && model.AssignedTo < model.AssignedFrom)
        {
            ModelState.AddModelError(
                $"{nameof(ProjectCrewManagementViewModel.Assignment)}.{nameof(model.AssignedTo)}",
                ProjectErrorMessages.InvalidAssignmentDates);
        }

        if (model.AssignedFrom is not null &&
            (model.AssignedFrom < project.StartDate ||
             (project.EndDate is not null && model.AssignedFrom > project.EndDate) ||
             (model.AssignedTo is not null && project.EndDate is not null && model.AssignedTo > project.EndDate)))
        {
            ModelState.AddModelError(
                $"{nameof(ProjectCrewManagementViewModel.Assignment)}.{nameof(model.AssignedFrom)}",
                ProjectErrorMessages.AssignmentOutsideProjectDates);
        }

        if (model.CrewId is not null && model.AssignedFrom is not null &&
            await dbContext.ProjectCrews.AnyAsync(assignment =>
                assignment.ProjectId == project.Id &&
                assignment.CrewId == model.CrewId &&
                assignment.AssignedFrom == model.AssignedFrom))
        {
            ModelState.AddModelError(
                $"{nameof(ProjectCrewManagementViewModel.Assignment)}.{nameof(model.AssignedFrom)}",
                ProjectErrorMessages.DuplicateCrewAssignment);
        }
    }

    private async Task PopulateTechnicalManagersAsync(ProjectFormViewModel model, int companyId)
    {
        model.TechnicalManagers = await dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.CompanyId == companyId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.TechnicalManager)
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .Select(employee => new EmployeeOptionViewModel
            {
                Id = employee.Id,
                FullName = employee.FirstName + " " + employee.LastName
            })
            .ToListAsync();
    }

    private async Task<ProjectCrewManagementViewModel?> BuildCrewManagementModelAsync(
        int projectId,
        int companyId,
        ProjectCrewAssignmentInputModel? assignment = null)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == projectId && item.CompanyId == companyId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.StartDate,
                item.EndDate
            })
            .SingleOrDefaultAsync();

        if (project is null)
        {
            return null;
        }

        var model = new ProjectCrewManagementViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectStartDate = project.StartDate,
            ProjectEndDate = project.EndDate,
            Assignment = assignment ?? new ProjectCrewAssignmentInputModel
            {
                ProjectId = project.Id,
                AssignedFrom = project.StartDate
            },
            AvailableCrews = await dbContext.Crews
                .AsNoTracking()
                .Where(crew => crew.CompanyId == companyId && crew.IsActive)
                .OrderBy(crew => crew.Name)
                .Select(crew => new CrewOptionViewModel
                {
                    Id = crew.Id,
                    Name = crew.Name
                })
                .ToListAsync(),
            Assignments = await dbContext.ProjectCrews
                .AsNoTracking()
                .Where(item => item.ProjectId == project.Id)
                .OrderByDescending(item => item.IsActive)
                .ThenByDescending(item => item.AssignedFrom)
                .Select(item => new ProjectCrewListItemViewModel
                {
                    Id = item.Id,
                    CrewName = item.Crew.Name,
                    ForemanName = item.Crew.Foreman.FirstName + " " + item.Crew.Foreman.LastName,
                    AssignedFrom = item.AssignedFrom,
                    AssignedTo = item.AssignedTo,
                    IsActive = item.IsActive
                })
                .ToListAsync()
        };

        return model;
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

    private static void NormalizeInput(ProjectFormViewModel model)
    {
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.ClientName = model.ClientName?.Trim() ?? string.Empty;
        model.Address = model.Address?.Trim() ?? string.Empty;
    }

    private sealed record OwnerCompanyContext(int CompanyId, string CompanyName);
}
