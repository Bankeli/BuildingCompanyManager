namespace BuildingCompanyManager.Models.Attendance;

public class AttendanceEmployeeDetailsViewModel
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

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

    public bool IsToday { get; set; }
}
