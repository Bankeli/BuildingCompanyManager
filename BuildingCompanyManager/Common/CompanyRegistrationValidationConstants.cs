namespace BuildingCompanyManager.Common;

public static class CompanyRegistrationValidationConstants
{
    public const int CompanyNameMaxLength = 100;
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int AddressMinLength = 5;
    public const int AddressMaxLength = 200;
    public const int PasswordMinLength = 6;
    public const int PasswordMaxLength = 100;

    public const string RegistrationNumberPattern = @"^(\d{9}|\d{13})$";
    public const string RegistrationNumberError = "EIK must contain exactly 9 or 13 digits.";
    public const string PasswordLengthError = "{0} must be at least {2} characters long.";
    public const string PasswordConfirmationError = "The password and confirmation password do not match.";
}
