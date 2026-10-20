using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Data.Common;

namespace BuildingCompanyManager.Models.Attendance;

public class AttendanceEditInputModel
{
    [Required]
    public int Id { get; set; }

    public string WorkerName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string CrewName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
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
}
