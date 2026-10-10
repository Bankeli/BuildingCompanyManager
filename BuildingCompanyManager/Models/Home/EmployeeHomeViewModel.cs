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

    public IReadOnlyCollection<ForemanCrewAssignmentViewModel> ForemanCrewAssignments { get; set; } = [];

    public EmployeeCrewAssignmentViewModel? CrewAssignment { get; set; }
}

public class ForemanCrewAssignmentViewModel
{
    public string CrewName { get; set; } = string.Empty;

    public string TechnicalManagerName { get; set; } = string.Empty;

    public IReadOnlyCollection<EmployeeProjectAssignmentViewModel> ProjectAssignments { get; set; } = [];
}

public class EmployeeCrewAssignmentViewModel
{
    public string CrewName { get; set; } = string.Empty;

    public string TechnicalManagerName { get; set; } = string.Empty;

    public string ForemanName { get; set; } = string.Empty;

    public IReadOnlyCollection<EmployeeProjectAssignmentViewModel> ProjectAssignments { get; set; } = [];
}

public class EmployeeProjectAssignmentViewModel
{
    public string ProjectName { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public DateOnly AssignedFrom { get; set; }

    public DateOnly? AssignedTo { get; set; }
}
