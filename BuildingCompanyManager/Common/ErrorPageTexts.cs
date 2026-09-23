namespace BuildingCompanyManager.Common;

public static class ErrorPageTexts
{
    public const string PageTitle = "Error";
    public const string Heading = "Error.";
    public const string Description = "An error occurred while processing your request.";
    public const string RequestId = "Request ID:";
    public const string DevelopmentMode = "Development Mode";
    public const string DevelopmentInformation = "Swapping to Development environment will display more detailed information about the error that occurred.";
    public const string DevelopmentWarning = "The Development environment shouldn't be enabled for deployed applications.";
    public const string SensitiveInformationWarning = "It can result in displaying sensitive information from exceptions to end users.";
    public const string LocalDebuggingInstructions = "For local debugging, enable the Development environment by setting the ASPNETCORE_ENVIRONMENT environment variable to Development and restarting the app.";
}
