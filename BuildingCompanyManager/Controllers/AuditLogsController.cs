using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Models.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class AuditLogsController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);
        var owner = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.UserId == userId && employee.IsActive && employee.Role == EmployeeRole.Owner)
            .Select(employee => new { employee.CompanyId, CompanyName = employee.Company.Name })
            .SingleOrDefaultAsync();

        if (owner is null)
        {
            return Forbid();
        }

        var model = new AuditLogIndexViewModel
        {
            CompanyName = owner.CompanyName,
            Entries = await dbContext.AuditLogs
                .AsNoTracking()
                .Where(log => log.CompanyId == owner.CompanyId)
                .OrderByDescending(log => log.CreatedOn)
                .Take(100)
                .Select(log => new AuditLogItemViewModel
                {
                    CreatedOn = log.CreatedOn,
                    ActorName = log.ActorEmployee.FirstName + " " + log.ActorEmployee.LastName,
                    Action = log.Action,
                    Description = log.Description
                })
                .ToListAsync()
        };

        return View(model);
    }
}
