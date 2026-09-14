using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingCompanyManager.Data.Models;

public class ProjectCrew
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int CrewId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Assigned from")]
    public DateOnly AssignedFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Assigned to")]
    public DateOnly? AssignedTo { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;

    [ForeignKey(nameof(CrewId))]
    public Crew Crew { get; set; } = null!;

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new HashSet<AttendanceRecord>();
}
