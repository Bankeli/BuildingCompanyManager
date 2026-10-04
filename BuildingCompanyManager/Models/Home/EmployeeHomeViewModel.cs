using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Home;

public class EmployeeHomeViewModel
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; }

    public decimal DailyRate { get; set; }

    public decimal MonthlyWorkedHours { get; set; }

    public decimal MonthlyEarnings { get; set; }
}
