namespace BuildingCompanyManager.Models.Attendance;

public class AttendanceEmployeeDetailsViewModel
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeInitials { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal TotalWorkedHours { get; set; }

    public bool CanReturnToAttendance { get; set; }

    public bool CanNavigateToPreviousMonth { get; set; }

    public bool CanNavigateToNextMonth { get; set; }

    public IReadOnlyCollection<AttendanceCalendarDayViewModel> CalendarDays { get; set; } = [];
}

public class AttendanceCalendarDayViewModel
{
    public DateOnly? Date { get; set; }

    public decimal? WorkedHours { get; set; }

    public decimal BonusAmount { get; set; }

    public string? ForemanComment { get; set; }

    public bool IsToday { get; set; }

    public bool IsWeekend => Date?.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public bool HasBonus => BonusAmount > 0;

    public bool HasComment => !string.IsNullOrWhiteSpace(ForemanComment);
}

public class AttendanceDayRecordViewModel
{
    public DateOnly WorkDate { get; set; }

    public decimal WorkedHours { get; set; }

    public decimal BonusAmount { get; set; }

    public string? ForemanComment { get; set; }
}
