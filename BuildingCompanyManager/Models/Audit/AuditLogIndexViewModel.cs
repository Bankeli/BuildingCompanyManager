using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Models.Audit;

public class AuditLogIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public IReadOnlyCollection<AuditLogItemViewModel> Entries { get; set; } = [];
}

public class AuditLogItemViewModel
{
    public DateTime CreatedOn { get; set; }

    public string ActorName { get; set; } = string.Empty;

    public AuditActionType Action { get; set; }

    public string Description { get; set; } = string.Empty;
}
