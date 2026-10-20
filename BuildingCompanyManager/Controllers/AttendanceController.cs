using System.Globalization;
using BuildingCompanyManager.Common;
using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;
using BuildingCompanyManager.Models.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public AttendanceController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? workDate)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee is null || !CanManageAttendance(currentEmployee.Role))
        {
            return Forbid();
        }

        var selectedDate = workDate ?? DateOnly.FromDateTime(DateTime.Today);
        var employees = await GetAccessibleEmployeesQuery(currentEmployee)
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .Select(employee => new
            {
                employee.Id,
                FullName = employee.FirstName + " " + employee.LastName,
                employee.Role
            })
            .ToListAsync();

        var recordsByEmployeeId = await GetAccessibleAttendanceRecordsQuery(currentEmployee)
            .AsNoTracking()
            .Where(record => record.WorkDate == selectedDate)
            .Select(record => new
            {
                record.Id,
                record.EmployeeId,
                record.WorkedHours,
                record.ExtraHours,
                record.BonusAmount
            })
            .ToDictionaryAsync(record => record.EmployeeId);

        var canModify = CanModifyWorkDate(currentEmployee, selectedDate);
        var model = new AttendanceIndexViewModel
        {
            CompanyName = currentEmployee.CompanyName,
            WorkDate = selectedDate,
            CanRecordHours = canModify,
            Employees = employees.Select(employee =>
            {
                recordsByEmployeeId.TryGetValue(employee.Id, out var record);

                return new AttendanceEmployeeListItemViewModel
                {
                    EmployeeId = employee.Id,
                    AttendanceRecordId = record?.Id,
                    FullName = employee.FullName,
                    Role = employee.Role,
                    WorkedHours = record?.WorkedHours ?? 0m,
                    ExtraHours = record?.ExtraHours ?? 0m,
                    BonusAmount = record?.BonusAmount ?? 0m,
                    CanEdit = record is not null && canModify
                };
            }).ToArray()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int employeeId, DateOnly? workDate)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee is null || !CanManageAttendance(currentEmployee.Role))
        {
            return Forbid();
        }

        var selectedDate = workDate ?? DateOnly.FromDateTime(DateTime.Today);

        if (!CanModifyWorkDate(currentEmployee, selectedDate))
        {
            return Forbid();
        }

        var model = new AttendanceCreateInputModel
        {
            EmployeeId = employeeId,
            WorkDate = selectedDate
        };

        if (!await PopulateCreateModelAsync(model, currentEmployee))
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttendanceCreateInputModel model)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee is null || !CanManageAttendance(currentEmployee.Role))
        {
            return Forbid();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);
        ValidateWorkDate(model.WorkDate, currentEmployee);
        ValidateWorkedHours(model.WorkedHours, model.ExtraHours, nameof(model.ExtraHours));

        var employee = await GetAccessibleEmployeesQuery(currentEmployee)
            .SingleOrDefaultAsync(item => item.Id == model.EmployeeId);

        if (employee is null)
        {
            ModelState.AddModelError(nameof(model.EmployeeId), AttendanceErrorMessages.InvalidWorker);
        }
        else if (employee.DailyRate is null)
        {
            ModelState.AddModelError(nameof(model.EmployeeId), AttendanceErrorMessages.DailyRateNotSet);
        }

        ProjectCrew? assignment = null;

        if (employee is not null && model.ProjectCrewId is not null)
        {
            assignment = await GetAccessibleProjectCrewsQuery(currentEmployee, employee.Id, model.WorkDate)
                .SingleOrDefaultAsync(item => item.Id == model.ProjectCrewId);

            if (assignment is null)
            {
                ModelState.AddModelError(nameof(model.ProjectCrewId), AttendanceErrorMessages.InvalidProjectCrewAssignment);
            }
        }

        if (employee is not null && await dbContext.AttendanceRecords.AnyAsync(record =>
                record.EmployeeId == employee.Id &&
                record.WorkDate == model.WorkDate))
        {
            ModelState.AddModelError(nameof(model.EmployeeId), AttendanceErrorMessages.DuplicateAttendanceRecord);
        }

        if (!ModelState.IsValid)
        {
            await PopulateCreateModelAsync(model, currentEmployee);

            return View(model);
        }

        var record = new AttendanceRecord
        {
            EmployeeId = employee!.Id,
            ProjectCrewId = assignment!.Id,
            RecordedByEmployeeId = currentEmployee.EmployeeId,
            WorkDate = model.WorkDate,
            WorkedHours = model.WorkedHours,
            IsPresent = model.WorkedHours > 0,
            DailyRateSnapshot = employee.DailyRate!.Value,
            ExtraHours = model.ExtraHours,
            BonusAmount = model.BonusAmount,
            ForemanComment = model.ForemanComment
        };

        dbContext.AttendanceRecords.Add(record);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, AttendanceErrorMessages.AttendanceSaveFailed);
            await PopulateCreateModelAsync(model, currentEmployee);

            return View(model);
        }

        TempData["SuccessMessage"] = "Working hours recorded successfully.";

        return RedirectToAction(nameof(Index), new { workDate = model.WorkDate });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee is null || !CanManageAttendance(currentEmployee.Role))
        {
            return Forbid();
        }

        var model = await GetAccessibleAttendanceRecordsQuery(currentEmployee)
            .AsNoTracking()
            .Where(record => record.Id == id)
            .Select(record => new AttendanceEditInputModel
            {
                Id = record.Id,
                WorkerName = record.Employee.FirstName + " " + record.Employee.LastName,
                ProjectName = record.ProjectCrew.Project.Name,
                CrewName = record.ProjectCrew.Crew.Name,
                WorkDate = record.WorkDate,
                WorkedHours = record.WorkedHours,
                ExtraHours = record.ExtraHours,
                BonusAmount = record.BonusAmount,
                ForemanComment = record.ForemanComment
            })
            .SingleOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        if (!CanModifyWorkDate(currentEmployee, model.WorkDate))
        {
            return Forbid();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AttendanceEditInputModel model)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee is null || !CanManageAttendance(currentEmployee.Role))
        {
            return Forbid();
        }

        var record = await GetAccessibleAttendanceRecordsQuery(currentEmployee)
            .SingleOrDefaultAsync(item => item.Id == model.Id);

        if (record is null)
        {
            return NotFound();
        }

        if (!CanModifyWorkDate(currentEmployee, record.WorkDate))
        {
            return Forbid();
        }

        NormalizeInput(model);
        ModelState.Clear();
        TryValidateModel(model);
        ValidateWorkedHours(model.WorkedHours, model.ExtraHours, nameof(model.ExtraHours));

        if (!ModelState.IsValid)
        {
            await PopulateEditDetailsAsync(model, record.Id, currentEmployee);

            return View(model);
        }

        record.WorkedHours = model.WorkedHours;
        record.IsPresent = model.WorkedHours > 0;
        record.ExtraHours = model.ExtraHours;
        record.BonusAmount = model.BonusAmount;
        record.ForemanComment = model.ForemanComment;
        record.ModifiedOn = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, AttendanceErrorMessages.AttendanceSaveFailed);
            await PopulateEditDetailsAsync(model, record.Id, currentEmployee);

            return View(model);
        }

        TempData["SuccessMessage"] = "Working hours updated successfully.";

        return RedirectToAction(nameof(Index), new { workDate = record.WorkDate });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int employeeId, int? year, int? month)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee is null)
        {
            return Forbid();
        }

        var canManageAttendance = CanManageAttendance(currentEmployee.Role);

        if (!canManageAttendance && employeeId != currentEmployee.EmployeeId)
        {
            return Forbid();
        }

        var employeesQuery = canManageAttendance
            ? GetAccessibleEmployeesQuery(currentEmployee)
            : dbContext.Employees.AsNoTracking().Where(item =>
                item.Id == currentEmployee.EmployeeId &&
                item.CompanyId == currentEmployee.CompanyId &&
                item.IsActive);

        var employee = await employeesQuery
            .AsNoTracking()
            .Where(item => item.Id == employeeId)
            .Select(item => new
            {
                item.Id,
                item.FirstName,
                item.LastName
            })
            .SingleOrDefaultAsync();

        if (employee is null)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var selectedYear = year is >= 1 and <= 9999 ? year.Value : today.Year;
        var selectedMonth = month is >= 1 and <= 12 ? month.Value : today.Month;
        var monthStart = new DateOnly(selectedYear, selectedMonth, 1);
        var monthEnd = new DateOnly(
            selectedYear,
            selectedMonth,
            DateTime.DaysInMonth(selectedYear, selectedMonth));

        var recordsQuery = canManageAttendance
            ? GetAccessibleAttendanceRecordsQuery(currentEmployee)
            : dbContext.AttendanceRecords.Where(record =>
                record.EmployeeId == currentEmployee.EmployeeId &&
                record.ProjectCrew.Project.CompanyId == currentEmployee.CompanyId);

        var records = await recordsQuery
            .AsNoTracking()
            .Where(record =>
                record.EmployeeId == employee.Id &&
                record.WorkDate >= monthStart &&
                record.WorkDate <= monthEnd)
            .Select(record => new AttendanceDayRecordViewModel
            {
                WorkDate = record.WorkDate,
                WorkedHours = record.WorkedHours,
                BonusAmount = record.BonusAmount,
                ForemanComment = record.ForemanComment
            })
            .ToDictionaryAsync(record => record.WorkDate);

        var model = new AttendanceEmployeeDetailsViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FirstName + " " + employee.LastName,
            EmployeeInitials = GetInitials(employee.FirstName, employee.LastName),
            CompanyName = currentEmployee.CompanyName,
            Year = selectedYear,
            Month = selectedMonth,
            MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(selectedMonth),
            TotalWorkedHours = records.Values.Sum(record => record.WorkedHours),
            CanReturnToAttendance = canManageAttendance,
            CanNavigateToPreviousMonth = monthStart != DateOnly.MinValue,
            CanNavigateToNextMonth = monthStart.Year != 9999 || monthStart.Month != 12,
            CalendarDays = BuildCalendarDays(monthStart, records, today)
        };

        return View(model);
    }

    private IQueryable<Employee> GetAccessibleEmployeesQuery(EmployeeContext currentEmployee)
    {
        var query = dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.CompanyId == currentEmployee.CompanyId &&
                employee.IsActive &&
                employee.Role != EmployeeRole.Owner);

        return currentEmployee.Role switch
        {
            EmployeeRole.Owner => query,
            EmployeeRole.TechnicalManager => query.Where(employee =>
                employee.Id == currentEmployee.EmployeeId ||
                dbContext.ProjectCrews.Any(assignment =>
                    assignment.IsActive &&
                    assignment.Project.TechnicalManagerId == currentEmployee.EmployeeId &&
                    (assignment.Crew.ForemanId == employee.Id ||
                     assignment.Crew.TechnicalManagerId == employee.Id ||
                     assignment.Crew.Members.Any(member => member.Id == employee.Id)))),
            EmployeeRole.Foreman => query.Where(employee =>
                dbContext.Crews.Any(crew =>
                    crew.IsActive &&
                    crew.ForemanId == currentEmployee.EmployeeId &&
                    (crew.ForemanId == employee.Id ||
                     crew.Members.Any(member => member.Id == employee.Id)))),
            _ => query.Where(_ => false)
        };
    }

    private IQueryable<AttendanceRecord> GetAccessibleAttendanceRecordsQuery(EmployeeContext currentEmployee)
    {
        var query = dbContext.AttendanceRecords
            .Where(record => record.ProjectCrew.Project.CompanyId == currentEmployee.CompanyId);

        return currentEmployee.Role switch
        {
            EmployeeRole.Owner => query,
            EmployeeRole.TechnicalManager => query.Where(record =>
                record.ProjectCrew.Project.TechnicalManagerId == currentEmployee.EmployeeId),
            EmployeeRole.Foreman => query.Where(record =>
                record.ProjectCrew.Crew.ForemanId == currentEmployee.EmployeeId &&
                record.Employee.Role != EmployeeRole.TechnicalManager),
            _ => query.Where(_ => false)
        };
    }

    private IQueryable<ProjectCrew> GetAccessibleProjectCrewsQuery(
        EmployeeContext currentEmployee,
        int employeeId,
        DateOnly workDate)
    {
        var query = dbContext.ProjectCrews
            .AsNoTracking()
            .Where(assignment =>
                assignment.Project.CompanyId == currentEmployee.CompanyId &&
                assignment.AssignedFrom <= workDate &&
                (assignment.AssignedTo == null || assignment.AssignedTo >= workDate) &&
                (assignment.Crew.ForemanId == employeeId ||
                 assignment.Crew.TechnicalManagerId == employeeId ||
                 assignment.Project.TechnicalManagerId == employeeId ||
                 assignment.Crew.Members.Any(member => member.Id == employeeId)));

        if (currentEmployee.Role == EmployeeRole.Owner &&
            workDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return query;
        }

        query = query.Where(assignment =>
            assignment.IsActive &&
            assignment.Project.Status == ProjectStatus.Active);

        return currentEmployee.Role switch
        {
            EmployeeRole.Owner => query,
            EmployeeRole.TechnicalManager => query.Where(assignment =>
                assignment.Project.TechnicalManagerId == currentEmployee.EmployeeId),
            EmployeeRole.Foreman => query.Where(assignment =>
                assignment.Crew.ForemanId == currentEmployee.EmployeeId),
            _ => query.Where(_ => false)
        };
    }

    private async Task<bool> PopulateCreateModelAsync(
        AttendanceCreateInputModel model,
        EmployeeContext currentEmployee)
    {
        var employee = await GetAccessibleEmployeesQuery(currentEmployee)
            .Where(item => item.Id == model.EmployeeId)
            .Select(item => new
            {
                FullName = item.FirstName + " " + item.LastName
            })
            .SingleOrDefaultAsync();

        if (employee is null)
        {
            model.AvailableProjectCrews = [];

            return false;
        }

        model.EmployeeName = employee.FullName;
        model.AvailableProjectCrews = await GetAccessibleProjectCrewsQuery(
                currentEmployee,
                model.EmployeeId,
                model.WorkDate)
            .OrderBy(assignment => assignment.Project.Name)
            .ThenBy(assignment => assignment.Crew.Name)
            .Select(assignment => new AttendanceProjectCrewOptionViewModel
            {
                Id = assignment.Id,
                ProjectName = assignment.Project.Name,
                CrewName = assignment.Crew.Name
            })
            .ToListAsync();

        if (model.ProjectCrewId is null && model.AvailableProjectCrews.Count == 1)
        {
            model.ProjectCrewId = model.AvailableProjectCrews.Single().Id;
        }

        return true;
    }

    private async Task PopulateEditDetailsAsync(
        AttendanceEditInputModel model,
        int recordId,
        EmployeeContext currentEmployee)
    {
        var details = await GetAccessibleAttendanceRecordsQuery(currentEmployee)
            .AsNoTracking()
            .Where(record => record.Id == recordId)
            .Select(record => new
            {
                WorkerName = record.Employee.FirstName + " " + record.Employee.LastName,
                ProjectName = record.ProjectCrew.Project.Name,
                CrewName = record.ProjectCrew.Crew.Name,
                record.WorkDate
            })
            .SingleOrDefaultAsync();

        if (details is null)
        {
            return;
        }

        model.WorkerName = details.WorkerName;
        model.ProjectName = details.ProjectName;
        model.CrewName = details.CrewName;
        model.WorkDate = details.WorkDate;
    }

    private async Task<EmployeeContext?> GetCurrentEmployeeAsync()
    {
        var userId = userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.UserId == userId && employee.IsActive)
            .Select(employee => new EmployeeContext(
                employee.Id,
                employee.CompanyId,
                employee.Company.Name,
                employee.Role))
            .SingleOrDefaultAsync();
    }

    private static IReadOnlyCollection<AttendanceCalendarDayViewModel> BuildCalendarDays(
        DateOnly monthStart,
        IReadOnlyDictionary<DateOnly, AttendanceDayRecordViewModel> records,
        DateOnly today)
    {
        var firstDayOffset = ((int)monthStart.DayOfWeek + 6) % 7;
        var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
        var totalCells = ((firstDayOffset + daysInMonth + 6) / 7) * 7;
        var calendarDays = new List<AttendanceCalendarDayViewModel>(totalCells);

        for (var index = 0; index < totalCells; index++)
        {
            var dayNumber = index - firstDayOffset + 1;

            if (dayNumber < 1 || dayNumber > daysInMonth)
            {
                calendarDays.Add(new AttendanceCalendarDayViewModel());
                continue;
            }

            var date = new DateOnly(monthStart.Year, monthStart.Month, dayNumber);
            records.TryGetValue(date, out var record);

            calendarDays.Add(new AttendanceCalendarDayViewModel
            {
                Date = date,
                WorkedHours = record?.WorkedHours ?? 0m,
                BonusAmount = record?.BonusAmount ?? 0m,
                ForemanComment = record?.ForemanComment,
                IsToday = date == today
            });
        }

        return calendarDays;
    }

    private static bool CanManageAttendance(EmployeeRole role) =>
        role is EmployeeRole.Owner or EmployeeRole.TechnicalManager or EmployeeRole.Foreman;

    private static bool CanModifyWorkDate(EmployeeContext currentEmployee, DateOnly workDate) =>
        workDate <= DateOnly.FromDateTime(DateTime.Today) &&
        (currentEmployee.Role == EmployeeRole.Owner || workDate == DateOnly.FromDateTime(DateTime.Today));

    private void ValidateWorkDate(DateOnly workDate, EmployeeContext currentEmployee)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (workDate == default || workDate > today)
        {
            ModelState.AddModelError(nameof(AttendanceCreateInputModel.WorkDate), AttendanceErrorMessages.InvalidWorkDate);
        }

        if (workDate < today && currentEmployee.Role != EmployeeRole.Owner)
        {
            ModelState.AddModelError(nameof(AttendanceCreateInputModel.WorkDate), AttendanceErrorMessages.PastDateEditingNotAllowed);
        }
    }

    private void ValidateWorkedHours(decimal workedHours, decimal extraHours, string extraHoursPropertyName)
    {
        if (extraHours > workedHours)
        {
            ModelState.AddModelError(extraHoursPropertyName, AttendanceErrorMessages.ExtraHoursExceedWorkedHours);
        }
    }

    private static void NormalizeInput(AttendanceCreateInputModel model)
    {
        model.ForemanComment = NormalizeComment(model.ForemanComment);
    }

    private static void NormalizeInput(AttendanceEditInputModel model)
    {
        model.ForemanComment = NormalizeComment(model.ForemanComment);
    }

    private static string? NormalizeComment(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

    private static string GetInitials(string firstName, string lastName) =>
        string.Concat(firstName.FirstOrDefault(), lastName.FirstOrDefault()).ToUpperInvariant();

    private sealed record EmployeeContext(
        int EmployeeId,
        int CompanyId,
        string CompanyName,
        EmployeeRole Role);
}
