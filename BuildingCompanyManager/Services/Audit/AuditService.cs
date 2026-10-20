using BuildingCompanyManager.Data;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;

namespace BuildingCompanyManager.Services.Audit;

public class AuditService(ApplicationDbContext dbContext) : IAuditService
{
    public void Record(int companyId, int actorEmployeeId, AuditActionType action, string entityType, int entityId, string description)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            CompanyId = companyId,
            ActorEmployeeId = actorEmployeeId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description
        });
    }
}
