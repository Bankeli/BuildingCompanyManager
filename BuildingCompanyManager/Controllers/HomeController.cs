using System.Diagnostics;
using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Models;
using BuildingCompanyManager.Models.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return View();
            }

            var employee = await dbContext.Employees
                .AsNoTracking()
                .Where(item => item.UserId == userId && item.IsActive)
                .Select(item => new
                {
                    item.Id,
                    item.FirstName,
                    item.CompanyId,
                    CompanyName = item.Company.Name,
                    item.JobTitle,
                    item.Role,
                    item.DailyRate,
                    item.CrewId
                })
                .SingleOrDefaultAsync();

            if (employee is null)
            {
                return View();
            }

            if (employee.Role == EmployeeRole.Owner)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var monthlyTotals = await dbContext.AttendanceRecords
                .AsNoTracking()
                .Where(record =>
                    record.EmployeeId == employee.Id &&
                    record.WorkDate >= monthStart &&
                    record.WorkDate < monthEnd)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    WorkedHours = group.Sum(record => record.WorkedHours),
                    Earnings = group.Sum(record =>
                        (record.WorkedHours / 8m * record.DailyRateSnapshot) + record.BonusAmount)
                })
                .SingleOrDefaultAsync();

            var model = new EmployeeHomeViewModel
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                CompanyName = employee.CompanyName,
                JobTitle = employee.JobTitle,
                Role = employee.Role,
                DailyRate = employee.DailyRate ?? 0m,
                MonthlyWorkedHours = monthlyTotals?.WorkedHours ?? 0m,
                MonthlyEarnings = monthlyTotals?.Earnings ?? 0m
            };

            if (employee.Role == EmployeeRole.Foreman)
            {
                var crews = await dbContext.Crews
                    .AsNoTracking()
                    .Where(crew => crew.ForemanId == employee.Id && crew.IsActive)
                    .Select(crew => new
                    {
                        crew.Id,
                        crew.Name,
                        TechnicalManagerName = crew.TechnicalManager.FirstName + " " + crew.TechnicalManager.LastName
                    })
                    .ToListAsync();

                var crewIds = crews.Select(crew => crew.Id).ToArray();
                var projectAssignments = await GetCurrentProjectAssignmentsQuery(today)
                    .Where(assignment => crewIds.Contains(assignment.CrewId))
                    .Select(assignment => new
                    {
                        assignment.CrewId,
                        assignment.Project.Name,
                        assignment.Project.ClientName,
                        assignment.AssignedFrom,
                        assignment.AssignedTo
                    })
                    .ToListAsync();

                model.ForemanCrewAssignments = crews
                    .Select(crew => new ForemanCrewAssignmentViewModel
                    {
                        CrewName = crew.Name,
                        TechnicalManagerName = crew.TechnicalManagerName,
                        ProjectAssignments = projectAssignments
                            .Where(assignment => assignment.CrewId == crew.Id)
                            .OrderBy(assignment => assignment.Name)
                            .Select(assignment => new EmployeeProjectAssignmentViewModel
                            {
                                ProjectName = assignment.Name,
                                ClientName = assignment.ClientName,
                                AssignedFrom = assignment.AssignedFrom,
                                AssignedTo = assignment.AssignedTo
                            })
                            .ToArray()
                    })
                    .OrderBy(crew => crew.CrewName)
                    .ToArray();
            }

            if (employee.Role == EmployeeRole.Worker && employee.CrewId is int crewId)
            {
                var crewAssignment = await dbContext.Crews
                    .AsNoTracking()
                    .Where(crew =>
                        crew.Id == crewId &&
                        crew.CompanyId == employee.CompanyId &&
                        crew.IsActive)
                    .Select(crew => new EmployeeCrewAssignmentViewModel
                    {
                        CrewName = crew.Name,
                        TechnicalManagerName = crew.TechnicalManager.FirstName + " " + crew.TechnicalManager.LastName,
                        ForemanName = crew.Foreman.FirstName + " " + crew.Foreman.LastName
                    })
                    .SingleOrDefaultAsync();

                if (crewAssignment is not null)
                {
                    crewAssignment.ProjectAssignments = await GetCurrentProjectAssignmentsQuery(today)
                        .Where(assignment => assignment.CrewId == crewId)
                        .OrderBy(assignment => assignment.Project.Name)
                        .Select(assignment => new EmployeeProjectAssignmentViewModel
                        {
                            ProjectName = assignment.Project.Name,
                            ClientName = assignment.Project.ClientName,
                            AssignedFrom = assignment.AssignedFrom,
                            AssignedTo = assignment.AssignedTo
                        })
                        .ToArrayAsync();

                    model.CrewAssignment = crewAssignment;
                }
            }

            return View(model);
        }

        private IQueryable<Data.Models.ProjectCrew> GetCurrentProjectAssignmentsQuery(DateOnly today)
        {
            return dbContext.ProjectCrews
                .AsNoTracking()
                .Where(assignment =>
                    assignment.IsActive &&
                    assignment.Project.Status == ProjectStatus.Active &&
                    assignment.AssignedFrom <= today &&
                    (assignment.AssignedTo == null || assignment.AssignedTo >= today));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
