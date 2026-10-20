using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public DashboardController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var owner = await dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.UserId == userId &&
                employee.IsActive &&
                employee.Role == EmployeeRole.Owner)
            .Select(employee => new
            {
                OwnerFirstName = employee.FirstName,
                CompanyName = employee.Company.Name,
                employee.CompanyId,
                ActiveEmployeesCount = employee.Company.Employees.Count(member => member.IsActive),
                ActiveCrewsCount = employee.Company.Crews.Count(crew => crew.IsActive),
                TotalProjectsCount = employee.Company.Projects.Count,
                ActiveProjectsCount = employee.Company.Projects.Count(project => project.Status == ProjectStatus.Active)
            })
            .SingleOrDefaultAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var activeProjects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.CompanyId == owner.CompanyId && project.Status == ProjectStatus.Active)
            .OrderBy(project => project.Name)
            .Select(project => new DashboardProjectItemViewModel
            {
                Id = project.Id,
                Name = project.Name,
                ClientName = project.ClientName,
                TechnicalManagerName = project.TechnicalManager.FirstName + " " + project.TechnicalManager.LastName,
                ActiveCrewsCount = project.ProjectCrews.Count(assignment => assignment.IsActive)
            })
            .ToListAsync();

        var activeCrews = await dbContext.Crews
            .AsNoTracking()
            .Where(crew => crew.CompanyId == owner.CompanyId && crew.IsActive)
            .OrderBy(crew => crew.Name)
            .Select(crew => new DashboardCrewItemViewModel
            {
                Id = crew.Id,
                Name = crew.Name,
                ForemanName = crew.Foreman.FirstName + " " + crew.Foreman.LastName,
                WorkersCount = crew.Members.Count(member => member.IsActive)
            })
            .ToListAsync();

        var model = new DashboardViewModel
        {
            OwnerFirstName = owner.OwnerFirstName,
            CompanyName = owner.CompanyName,
            ActiveEmployeesCount = owner.ActiveEmployeesCount,
            ActiveCrewsCount = owner.ActiveCrewsCount,
            TotalProjectsCount = owner.TotalProjectsCount,
            ActiveProjectsCount = owner.ActiveProjectsCount,
            ActiveProjects = activeProjects,
            ActiveCrews = activeCrews
        };

        return View(model);
    }
}
