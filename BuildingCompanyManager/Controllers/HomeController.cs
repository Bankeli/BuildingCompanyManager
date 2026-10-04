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
                    CompanyName = item.Company.Name,
                    item.JobTitle,
                    item.Role,
                    item.DailyRate
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

            return View(new EmployeeHomeViewModel
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                CompanyName = employee.CompanyName,
                JobTitle = employee.JobTitle,
                Role = employee.Role,
                DailyRate = employee.DailyRate ?? 0m,
                MonthlyWorkedHours = monthlyTotals?.WorkedHours ?? 0m,
                MonthlyEarnings = monthlyTotals?.Earnings ?? 0m
            });
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
