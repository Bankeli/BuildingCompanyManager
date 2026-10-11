namespace BuildingCompanyManager.Common;

public static class EmployeeAccountErrorMessages
{
    public const string AccountNotFound = "The employee account could not be found.";
    public const string InactiveEmployeePasswordReset = "Only active employee accounts can receive a new password.";
    public const string PasswordResetFailed = "We could not reset this employee password. Please try again.";
    public const string StatusUpdateFailed = "We could not update this employee account status. Please try again.";
}
