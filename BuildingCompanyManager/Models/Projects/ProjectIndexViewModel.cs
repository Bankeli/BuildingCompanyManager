using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Projects;

public class ProjectIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public IReadOnlyCollection<ProjectListItemViewModel> Projects { get; set; } = [];
}

public class ProjectListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public string TechnicalManagerName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int ActiveCrewsCount { get; set; }
}
