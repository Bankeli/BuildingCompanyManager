namespace BuildingCompanyManager.Data.Common;

public static class DatabaseConstants
{
    public const string CompaniesTableName = "Companies";
    public const string EmployeesTableName = "Employees";
    public const string CrewsTableName = "Crews";
    public const string ProjectsTableName = "Projects";
    public const string ProjectCrewsTableName = "ProjectCrews";
    public const string AttendanceRecordsTableName = "AttendanceRecords";

    public const string ActiveCrewForemanFilter = "[IsActive] = 1";
    public const string ProjectCrewAssignmentDatesConstraintName = "CK_ProjectCrews_AssignmentDates";
    public const string ProjectCrewAssignmentDatesConstraintSql = "[AssignedTo] IS NULL OR [AssignedTo] >= [AssignedFrom]";
    public const string AttendanceExtraHoursConstraintName = "CK_AttendanceRecords_ExtraHours";
    public const string AttendanceExtraHoursConstraintSql = "[ExtraHours] >= 0 AND [ExtraHours] <= 24";
    public const string AttendanceBonusAmountConstraintName = "CK_AttendanceRecords_BonusAmount";
    public const string AttendanceBonusAmountConstraintSql = "[BonusAmount] >= 0";
}
