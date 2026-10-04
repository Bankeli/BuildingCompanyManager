using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingCompanyManager.Data.Common;
using BuildingCompanyManager.Data.Enums;

namespace BuildingCompanyManager.Data.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.UserIdMaxLength)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    public int? CrewId { get; set; }

    [Required]
    public EmployeeRole Role { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.JobTitleMaxLength)]
    [Display(Name = "Job title")]
    public string JobTitle { get; set; } = string.Empty;

    [Range(typeof(decimal), EntityValidationConstants.MinimumPositiveAmount, EntityValidationConstants.MaximumMoneyAmount)]
    [Column(TypeName = EntityValidationConstants.MoneyColumnType)]
    [Display(Name = "Daily rate")]
    public decimal? DailyRate { get; set; }

    [Required]
    [StringLength(EntityValidationConstants.PersonNameMaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityValidationConstants.PersonNameMaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [StringLength(EntityValidationConstants.PhoneNumberMaxLength)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(CrewId))]
    public Crew? Crew { get; set; }

    public ICollection<Crew> ManagedCrews { get; set; } = new HashSet<Crew>();

    public ICollection<Project> ManagedProjects { get; set; } = new HashSet<Project>();

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new HashSet<AttendanceRecord>();

    public ICollection<AttendanceRecord> RecordedAttendanceRecords { get; set; } = new HashSet<AttendanceRecord>();

}
