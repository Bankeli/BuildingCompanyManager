using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Employees;

public class EmployeeDetailsViewModel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public EmployeeRole Role { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public decimal? DailyRate { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyCollection<EmployeeCrewAssignmentViewModel> CrewAssignments { get; set; } = [];

    public IReadOnlyCollection<EmployeeProjectItemViewModel> ProjectAssignments { get; set; } = [];
}

public class EmployeeCrewAssignmentViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TechnicalManagerName { get; set; } = string.Empty;

    public string ForemanName { get; set; } = string.Empty;
}

public class EmployeeProjectItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public bool IsTechnicalManager { get; set; }

    public IReadOnlyCollection<string> CrewNames { get; set; } = [];
}
