using BuildingCompanyManager.Data.Models;
using BuildingCompanyManager.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Crew> Crews => Set<Crew>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectCrew> ProjectCrews => Set<ProjectCrew>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureCompany(builder);
        ConfigureEmployee(builder);
        ConfigureCrew(builder);
        ConfigureProject(builder);
        ConfigureProjectCrew(builder);
        ConfigureAttendanceRecord(builder);
    }

    private static void ConfigureCompany(ModelBuilder builder)
    {
        builder.Entity<Company>(entity =>
        {
            entity.ToTable(DatabaseConstants.CompaniesTableName);
            entity.HasIndex(company => company.RegistrationNumber).IsUnique();
            entity.HasIndex(company => company.Name).IsUnique();

            entity.HasMany(company => company.Employees)
                .WithOne(employee => employee.Company)
                .HasForeignKey(employee => employee.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(company => company.Crews)
                .WithOne(crew => crew.Company)
                .HasForeignKey(crew => crew.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(company => company.Projects)
                .WithOne(project => project.Company)
                .HasForeignKey(project => project.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEmployee(ModelBuilder builder)
    {
        builder.Entity<Employee>(entity =>
        {
            entity.ToTable(DatabaseConstants.EmployeesTableName);
            entity.Property(employee => employee.Role).HasConversion<int>();
            entity.HasIndex(employee => employee.UserId).IsUnique();
            entity.HasIndex(employee => new { employee.CompanyId, employee.Role });

            entity.HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<Employee>(employee => employee.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(employee => employee.Crew)
                .WithMany(crew => crew.Members)
                .HasForeignKey(employee => employee.CrewId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCrew(ModelBuilder builder)
    {
        builder.Entity<Crew>(entity =>
        {
            entity.ToTable(DatabaseConstants.CrewsTableName);
            entity.HasIndex(crew => crew.ForemanId)
                .IsUnique()
                .HasFilter(DatabaseConstants.ActiveCrewForemanFilter);

            entity.HasOne(crew => crew.TechnicalManager)
                .WithMany(employee => employee.ManagedCrews)
                .HasForeignKey(crew => crew.TechnicalManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(crew => crew.Foreman)
                .WithOne()
                .HasForeignKey<Crew>(crew => crew.ForemanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProject(ModelBuilder builder)
    {
        builder.Entity<Project>(entity =>
        {
            entity.ToTable(DatabaseConstants.ProjectsTableName);
            entity.Property(project => project.Status).HasConversion<int>();
            entity.HasIndex(project => new { project.CompanyId, project.Status });

            entity.HasOne(project => project.TechnicalManager)
                .WithMany(employee => employee.ManagedProjects)
                .HasForeignKey(project => project.TechnicalManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProjectCrew(ModelBuilder builder)
    {
        builder.Entity<ProjectCrew>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint(
                DatabaseConstants.ProjectCrewAssignmentDatesConstraintName,
                DatabaseConstants.ProjectCrewAssignmentDatesConstraintSql));

            entity.HasIndex(projectCrew => new
            {
                projectCrew.ProjectId,
                projectCrew.CrewId,
                projectCrew.AssignedFrom
            }).IsUnique();

            entity.HasOne(projectCrew => projectCrew.Project)
                .WithMany(project => project.ProjectCrews)
                .HasForeignKey(projectCrew => projectCrew.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(projectCrew => projectCrew.Crew)
                .WithMany(crew => crew.ProjectCrews)
                .HasForeignKey(projectCrew => projectCrew.CrewId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAttendanceRecord(ModelBuilder builder)
    {
        builder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    DatabaseConstants.AttendanceExtraHoursConstraintName,
                    DatabaseConstants.AttendanceExtraHoursConstraintSql);
                table.HasCheckConstraint(
                    DatabaseConstants.AttendanceBonusAmountConstraintName,
                    DatabaseConstants.AttendanceBonusAmountConstraintSql);
            });

            entity.HasIndex(record => new { record.EmployeeId, record.WorkDate }).IsUnique();
            entity.HasIndex(record => new { record.ProjectCrewId, record.WorkDate });

            entity.HasOne(record => record.Employee)
                .WithMany(employee => employee.AttendanceRecords)
                .HasForeignKey(record => record.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(record => record.MarkedByForeman)
                .WithMany(employee => employee.MarkedAttendanceRecords)
                .HasForeignKey(record => record.MarkedByForemanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(record => record.ProjectCrew)
                .WithMany(projectCrew => projectCrew.AttendanceRecords)
                .HasForeignKey(record => record.ProjectCrewId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
