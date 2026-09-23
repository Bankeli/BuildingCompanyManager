namespace BuildingCompanyManager.Common;

public static class ApplicationConstants
{
    public const string ApplicationName = "Builder Manager";
    public const string DefaultConnectionName = "DefaultConnection";
    public const string MissingConnectionStringMessage = "Connection string 'DefaultConnection' not found.";
    public const string ErrorHandlerPath = "/Home/Error";
    public const string DefaultRouteName = "default";
    public const string DefaultRoutePattern = "{controller=Home}/{action=Index}/{id?}";
    public const string ViewTitleKey = "Title";
}
