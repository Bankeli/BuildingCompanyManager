using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Range(typeof(decimal), "0.01", "999999.99")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Daily rate")]
    public decimal DailyRateSnapshot { get; set; }

    [Range(typeof(decimal), "0", "24")]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Extra hours")]
    public decimal ExtraHours { get; set; }

    [Range(typeof(decimal), "0", "999999.99")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Bonus amount")]
    public decimal BonusAmount { get; set; }

    [StringLength(1000)]
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
