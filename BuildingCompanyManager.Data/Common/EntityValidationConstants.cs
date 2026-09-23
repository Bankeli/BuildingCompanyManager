namespace BuildingCompanyManager.Data.Common;

public static class EntityValidationConstants
{
    public const int UserIdMaxLength = 450;
    public const int CompanyNameMaxLength = 100;
    public const int RegistrationNumberMaxLength = 20;
    public const int AddressMaxLength = 200;
    public const int JobTitleMaxLength = 100;
    public const int PersonNameMaxLength = 50;
    public const int PhoneNumberMaxLength = 20;
    public const int ProjectNameMaxLength = 150;
    public const int ForemanCommentMaxLength = 1000;

    public const string MinimumPositiveAmount = "0.01";
    public const string MinimumZeroAmount = "0";
    public const string MaximumMoneyAmount = "999999.99";
    public const string MaximumExtraHours = "24";
    public const string MoneyColumnType = "decimal(18,2)";
    public const string HoursColumnType = "decimal(5,2)";
}
