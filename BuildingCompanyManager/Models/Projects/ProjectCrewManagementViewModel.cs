using System.ComponentModel.DataAnnotations;
using BuildingCompanyManager.Models.Shared;

namespace BuildingCompanyManager.Models.Projects;

public class ProjectCrewManagementViewModel
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public DateOnly ProjectStartDate { get; set; }

    public DateOnly? ProjectEndDate { get; set; }

    public ProjectCrewAssignmentInputModel Assignment { get; set; } = new();

    public IReadOnlyCollection<CrewOptionViewModel> AvailableCrews { get; set; } = [];

    public IReadOnlyCollection<ProjectCrewListItemViewModel> Assignments { get; set; } = [];
}

public class ProjectCrewAssignmentInputModel
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [Display(Name = "Crew")]
    public int? CrewId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Assigned from")]
    public DateOnly? AssignedFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    [Display(Name = "Assigned to")]
    public DateOnly? AssignedTo { get; set; }
}

public class CrewOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class ProjectCrewListItemViewModel
{
    public int Id { get; set; }

    public string CrewName { get; set; } = string.Empty;

    public string ForemanName { get; set; } = string.Empty;

    public DateOnly AssignedFrom { get; set; }

    public DateOnly? AssignedTo { get; set; }

    public bool IsActive { get; set; }
}
