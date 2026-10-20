using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Common;

public static class DisplayNameExtensions
{
    public static string ToDisplayName(this EmployeeRole role) => role switch
    {
        EmployeeRole.Owner => "Owner",
        EmployeeRole.TechnicalManager => "Technical manager",
        EmployeeRole.Foreman => "Foreman",
        EmployeeRole.Worker => "Worker",
        _ => "Unknown"
    };

    public static string ToDisplayName(this ProjectStatus status) => status switch
    {
        ProjectStatus.Planning => "Planning",
        ProjectStatus.Active => "Active",
        ProjectStatus.OnHold => "On hold",
        ProjectStatus.Completed => "Completed",
        ProjectStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    public static string ToDisplayName(this AuditActionType action) => action switch
    {
        AuditActionType.EmployeeCreated => "Employee created",
        AuditActionType.EmployeeEmploymentUpdated => "Employment updated",
        AuditActionType.EmployeeAccountStatusChanged => "Account status changed",
        AuditActionType.EmployeePasswordReset => "Temporary password generated",
        AuditActionType.CrewAssignmentEnded => "Crew assignment ended",
        _ => "Unknown"
    };
}
