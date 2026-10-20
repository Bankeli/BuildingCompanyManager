using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Employees;

public class EmployeesIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public IReadOnlyCollection<EmployeeListItemViewModel> Employees { get; set; } = [];

    public IReadOnlyCollection<EmployeeFilterOptionViewModel> Crews { get; set; } = [];

    public IReadOnlyCollection<EmployeeFilterOptionViewModel> Projects { get; set; } = [];

    public int? SelectedCrewId { get; set; }

    public int? SelectedProjectId { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalEmployeesCount { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}

public class EmployeeFilterOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class EmployeeListItemViewModel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? CrewName { get; set; }

    public decimal? DailyRate { get; set; }

    public bool IsActive { get; set; }
}
