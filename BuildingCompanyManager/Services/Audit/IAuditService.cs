using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Services.Audit;

public interface IAuditService
{
    void Record(int companyId, int actorEmployeeId, AuditActionType action, string entityType, int entityId, string description);
}
