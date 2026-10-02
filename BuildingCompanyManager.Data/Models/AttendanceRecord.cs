using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingCompanyManager.Data.Common;

namespace BuildingCompanyManager.Data.Models;

public class AttendanceRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int ProjectCrewId { get; set; }

    [Required]
    [Display(Name = "Marked by foreman")]
    public int MarkedByForemanId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Work date")]
    public DateOnly WorkDate { get; set; }

    [Display(Name = "Present")]
    public bool IsPresent { get; set; }

    [Required]
    [Range(typeof(decimal), EntityValidationConstants.MinimumPositiveAmount, EntityValidationConstants.MaximumMoneyAmount)]
    [Column(TypeName = EntityValidationConstants.MoneyColumnType)]
    [Display(Name = "Daily rate")]
    public decimal DailyRateSnapshot { get; set; }

    [Range(typeof(decimal), EntityValidationConstants.MinimumZeroAmount, EntityValidationConstants.MaximumExtraHours)]
    [Column(TypeName = EntityValidationConstants.HoursColumnType)]
    [Display(Name = "Extra hours")]
    public decimal ExtraHours { get; set; }

    [Range(typeof(decimal), EntityValidationConstants.MinimumZeroAmount, EntityValidationConstants.MaximumMoneyAmount)]
    [Column(TypeName = EntityValidationConstants.MoneyColumnType)]
    [Display(Name = "Bonus amount")]
    public decimal BonusAmount { get; set; }

    [StringLength(EntityValidationConstants.ForemanCommentMaxLength)]
    [Display(Name = "Foreman comment")]
    public string? ForemanComment { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedOn { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(ProjectCrewId))]
    public ProjectCrew ProjectCrew { get; set; } = null!;

    [ForeignKey(nameof(MarkedByForemanId))]
    public Employee MarkedByForeman { get; set; } = null!;
}
