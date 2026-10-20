namespace BuildingCompanyManager.Common;

public static class EmployeeRoleChangeErrorMessages
{
    public const string InvalidRole = "Select a valid employee role.";
    public const string TechnicalManagerHasAssignments = "This employee cannot be changed from Technical Manager while assigned to {0} project(s) and {1} crew(s). Reassign them first.";
    public const string ForemanHasAssignments = "This employee cannot be changed from Foreman while responsible for {0} active crew(s). Assign another Foreman first.";
}
