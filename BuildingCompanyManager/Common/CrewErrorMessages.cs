namespace BuildingCompanyManager.Common;

public static class CrewErrorMessages
{
    public const string InvalidTechnicalManager = "Select an active Technical Manager from your company.";
    public const string InvalidForeman = "Select an available Foreman from your company.";
    public const string InvalidWorkers = "Selected workers must be active and available in your company.";
    public const string CrewNameAlreadyExists = "An active crew with this name already exists.";
    public const string CrewHasActiveAssignments = "End the active project assignments before deactivating this crew.";
    public const string CrewUpdateFailed = "We could not save this crew. Please try again.";
}
