using System.Globalization;
using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Models.Employees;
using BuildingCompanyManager.Models.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class PayrollController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public PayrollController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month, int? crewId, int? projectId)
    {
        var owner = await GetCurrentOwnerAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var selectedYear = year is >= 1 and <= 9999 ? year.Value : today.Year;
        var selectedMonth = month is >= 1 and <= 12 ? month.Value : today.Month;
        var monthStart = new DateOnly(selectedYear, selectedMonth, 1);
        var monthEnd = monthStart.AddMonths(1);
        var selectedCrewId = await GetValidCrewIdAsync(crewId, owner.CompanyId);
        var selectedProjectId = await GetValidProjectIdAsync(projectId, owner.CompanyId);

        var employees = await dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.CompanyId == owner.CompanyId &&
                employee.IsActive &&
                employee.Role != EmployeeRole.Owner)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => new
            {
                employee.Id,
                FullName = employee.FirstName + " " + employee.LastName,
                employee.JobTitle
            })
            .ToListAsync();

        var recordsQuery = dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(record =>
                record.Employee.CompanyId == owner.CompanyId &&
                record.WorkDate >= monthStart &&
                record.WorkDate < monthEnd);

        if (selectedCrewId is not null)
        {
            recordsQuery = recordsQuery.Where(record => record.ProjectCrew.CrewId == selectedCrewId);
        }

        if (selectedProjectId is not null)
        {
            recordsQuery = recordsQuery.Where(record => record.ProjectCrew.ProjectId == selectedProjectId);
        }

        var totals = await recordsQuery
            .GroupBy(record => record.EmployeeId)
            .Select(group => new
            {
                EmployeeId = group.Key,
                WorkedHours = group.Sum(record => record.WorkedHours),
                Bonuses = group.Sum(record => record.BonusAmount),
                TotalAmount = group.Sum(record =>
                    (record.WorkedHours / 8m * record.DailyRateSnapshot) + record.BonusAmount)
            })
            .ToDictionaryAsync(total => total.EmployeeId);

        var model = new PayrollIndexViewModel
        {
            CompanyName = owner.CompanyName,
            Year = selectedYear,
            Month = selectedMonth,
            MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(selectedMonth),
            SelectedCrewId = selectedCrewId,
            SelectedProjectId = selectedProjectId,
            Employees = employees.Select(employee =>
            {
                totals.TryGetValue(employee.Id, out var total);

                return new PayrollEmployeeItemViewModel
                {
                    FullName = employee.FullName,
                    JobTitle = employee.JobTitle,
                    WorkedHours = total?.WorkedHours ?? 0m,
                    Bonuses = total?.Bonuses ?? 0m,
                    TotalAmount = total?.TotalAmount ?? 0m
                };
            }).ToArray(),
            Crews = await GetCrewOptionsAsync(owner.CompanyId),
            Projects = await GetProjectOptionsAsync(owner.CompanyId)
        };

        return View(model);
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
            .Where(employee => employee.UserId == userId && employee.IsActive && employee.Role == EmployeeRole.Owner)
            .Select(employee => new OwnerCompanyContext(employee.CompanyId, employee.Company.Name))
            .SingleOrDefaultAsync();
    }

    private async Task<int?> GetValidCrewIdAsync(int? crewId, int companyId) =>
        crewId is not null && await dbContext.Crews.AnyAsync(crew =>
            crew.Id == crewId && crew.CompanyId == companyId && crew.IsActive)
            ? crewId
            : null;

    private async Task<int?> GetValidProjectIdAsync(int? projectId, int companyId) =>
        projectId is not null && await dbContext.Projects.AnyAsync(project =>
            project.Id == projectId && project.CompanyId == companyId)
            ? projectId
            : null;

    private async Task<IReadOnlyCollection<EmployeeFilterOptionViewModel>> GetCrewOptionsAsync(int companyId) =>
        await dbContext.Crews
            .AsNoTracking()
            .Where(crew => crew.CompanyId == companyId && crew.IsActive)
            .OrderBy(crew => crew.Name)
            .Select(crew => new EmployeeFilterOptionViewModel { Id = crew.Id, Name = crew.Name })
            .ToListAsync();

    private async Task<IReadOnlyCollection<EmployeeFilterOptionViewModel>> GetProjectOptionsAsync(int companyId) =>
        await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.CompanyId == companyId)
            .OrderBy(project => project.Name)
            .Select(project => new EmployeeFilterOptionViewModel { Id = project.Id, Name = project.Name })
            .ToListAsync();

    private sealed record OwnerCompanyContext(int CompanyId, string CompanyName);
}
