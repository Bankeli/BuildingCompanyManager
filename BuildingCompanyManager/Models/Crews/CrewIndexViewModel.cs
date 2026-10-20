namespace BuildingCompanyManager.Models.Crews;

public class CrewIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public IReadOnlyCollection<CrewListItemViewModel> Crews { get; set; } = [];
}

public class CrewListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TechnicalManagerName { get; set; } = string.Empty;

    public string ForemanName { get; set; } = string.Empty;

    public int WorkersCount { get; set; }

    public bool IsActive { get; set; }
}
