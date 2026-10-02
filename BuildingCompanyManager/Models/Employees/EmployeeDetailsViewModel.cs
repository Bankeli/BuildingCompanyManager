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

    public int? CrewId { get; set; }

    public string? CrewName { get; set; }

    public string? ForemanName { get; set; }

    public string? TechnicalManagerName { get; set; }

    public IReadOnlyCollection<EmployeeProjectItemViewModel> ActiveProjects { get; set; } = [];
}

public class EmployeeProjectItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;
}
