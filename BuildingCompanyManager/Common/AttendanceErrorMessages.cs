namespace BuildingCompanyManager.Common;

public static class AttendanceErrorMessages
{
    public const string InvalidWorkDate = "Working hours can only be recorded for today or an earlier date.";
    public const string PastDateEditingNotAllowed = "Only the owner can add or edit working hours for past dates.";
    public const string InvalidProjectCrewAssignment = "The selected project crew assignment is not available to you on this date.";
    public const string InvalidWorker = "The selected employee is not available to you.";
    public const string DailyRateNotSet = "The selected worker does not have a daily rate.";
    public const string DuplicateAttendanceRecord = "This worker already has a working-hours record for the selected date.";
    public const string ExtraHoursExceedWorkedHours = "Extra hours cannot be greater than total worked hours.";
    public const string AttendanceSaveFailed = "The working-hours record could not be saved. Please try again.";
}
