namespace BuildingCompanyManager.Common;

public static class ProjectErrorMessages
{
    public const string InvalidTechnicalManager = "Select an active Technical Manager from your company.";
    public const string InvalidProjectStatus = "Select a valid project status.";
    public const string InvalidProjectDates = "The end date cannot be earlier than the start date.";
    public const string ProjectNameAlreadyExists = "A project with this name already exists for your company.";
    public const string ProjectUpdateFailed = "We could not save this project. Please try again.";
    public const string InvalidCrew = "Select an active crew from your company.";
    public const string InvalidAssignmentDates = "The assignment end date cannot be earlier than its start date.";
    public const string AssignmentOutsideProjectDates = "The crew assignment must fall within the project dates.";
    public const string DuplicateCrewAssignment = "This crew already has an assignment starting on that date.";
}
