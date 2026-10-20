using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Attendance;

public class AttendanceIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public DateOnly WorkDate { get; set; }

    public bool CanRecordHours { get; set; }

    public IReadOnlyCollection<AttendanceEmployeeListItemViewModel> Employees { get; set; } = [];
}

public class AttendanceEmployeeListItemViewModel
{
    public int EmployeeId { get; set; }

    public int? AttendanceRecordId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; }

    public decimal WorkedHours { get; set; }

    public decimal ExtraHours { get; set; }

    public decimal BonusAmount { get; set; }

    public bool CanEdit { get; set; }
}
