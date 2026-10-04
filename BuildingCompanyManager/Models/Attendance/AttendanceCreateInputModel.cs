using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;

namespace BuildingCompanyManager.Models.Attendance;

public class AttendanceCreateInputModel
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Project assignment")]
    public int? ProjectCrewId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Work date")]
    public DateOnly WorkDate { get; set; }

    [Range(typeof(decimal), EntityValidationConstants.MinimumZeroAmount, EntityValidationConstants.MaximumExtraHours)]
    [Display(Name = "Worked hours")]
    public decimal WorkedHours { get; set; }

    [Range(typeof(decimal), EntityValidationConstants.MinimumZeroAmount, EntityValidationConstants.MaximumExtraHours)]
    [Display(Name = "Extra hours")]
    public decimal ExtraHours { get; set; }

    [Range(typeof(decimal), EntityValidationConstants.MinimumZeroAmount, EntityValidationConstants.MaximumMoneyAmount)]
    [Display(Name = "Bonus amount")]
    public decimal BonusAmount { get; set; }

    [StringLength(EntityValidationConstants.ForemanCommentMaxLength)]
    [Display(Name = "Comment")]
    public string? ForemanComment { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public IReadOnlyCollection<AttendanceProjectCrewOptionViewModel> AvailableProjectCrews { get; set; } = [];
}

public class AttendanceProjectCrewOptionViewModel
{
    public int Id { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string CrewName { get; set; } = string.Empty;
}
