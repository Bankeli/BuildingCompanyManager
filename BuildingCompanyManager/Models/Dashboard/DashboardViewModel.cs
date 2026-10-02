namespace BuildingCompanyManager.Models.Dashboard;

public class DashboardViewModel
{
    public string OwnerFirstName { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public int ActiveEmployeesCount { get; set; }

    public int ActiveCrewsCount { get; set; }

    public int TotalProjectsCount { get; set; }

    public int ActiveProjectsCount { get; set; }

    public IReadOnlyCollection<DashboardProjectItemViewModel> ActiveProjects { get; set; } = [];

    public IReadOnlyCollection<DashboardCrewItemViewModel> ActiveCrews { get; set; } = [];
}

public class DashboardProjectItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string TechnicalManagerName { get; set; } = string.Empty;

    public int ActiveCrewsCount { get; set; }
}

public class DashboardCrewItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ForemanName { get; set; } = string.Empty;

    public int WorkersCount { get; set; }
}
