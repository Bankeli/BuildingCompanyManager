using BuildingCompanyManager.Common;
using BuildingCompanyManager.Data.Enums;
using BuildingCompanyManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BuildingCompanyManager.Data.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        if (await dbContext.Companies.AnyAsync(company => company.Name == DevelopmentSeedConstants.CompanyName))
        {
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var company = new Company
            {
                Name = DevelopmentSeedConstants.CompanyName,
                RegistrationNumber = DevelopmentSeedConstants.RegistrationNumber,
                Address = "100 Demo Construction Avenue, Sofia"
            };

            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();

            var ownerUser = await CreateUserAsync(userManager, "owner@buildpro-demo.local");
            var firstTechnicalManagerUser = await CreateUserAsync(userManager, "thomas.hale@buildpro-demo.local");
            var secondTechnicalManagerUser = await CreateUserAsync(userManager, "elena.stone@buildpro-demo.local");
            var firstForemanUser = await CreateUserAsync(userManager, "peter.mills@buildpro-demo.local");
            var secondForemanUser = await CreateUserAsync(userManager, "maria.woods@buildpro-demo.local");
            var firstWorkerUser = await CreateUserAsync(userManager, "george.ross@buildpro-demo.local");
            var secondWorkerUser = await CreateUserAsync(userManager, "daniel.price@buildpro-demo.local");
            var thirdWorkerUser = await CreateUserAsync(userManager, "victor.king@buildpro-demo.local");
            var fourthWorkerUser = await CreateUserAsync(userManager, "stefan.baker@buildpro-demo.local");
            var fifthWorkerUser = await CreateUserAsync(userManager, "ivan.reed@buildpro-demo.local");

            var owner = CreateEmployee(ownerUser.Id, company.Id, "Oliver", "Grant", EmployeeRole.Owner, "Company Owner", null);
            var firstTechnicalManager = CreateEmployee(firstTechnicalManagerUser.Id, company.Id, "Thomas", "Hale", EmployeeRole.TechnicalManager, "Technical Manager", 260m);
            var secondTechnicalManager = CreateEmployee(secondTechnicalManagerUser.Id, company.Id, "Elena", "Stone", EmployeeRole.TechnicalManager, "Technical Manager", 250m);
            var firstForeman = CreateEmployee(firstForemanUser.Id, company.Id, "Peter", "Mills", EmployeeRole.Foreman, "Foreman", 220m);
            var secondForeman = CreateEmployee(secondForemanUser.Id, company.Id, "Maria", "Woods", EmployeeRole.Foreman, "Foreman", 215m);
            var firstWorker = CreateEmployee(firstWorkerUser.Id, company.Id, "George", "Ross", EmployeeRole.Worker, "Concrete Worker", 175m);
            var secondWorker = CreateEmployee(secondWorkerUser.Id, company.Id, "Daniel", "Price", EmployeeRole.Worker, "Steel Fixer", 180m);
            var thirdWorker = CreateEmployee(thirdWorkerUser.Id, company.Id, "Victor", "King", EmployeeRole.Worker, "Crane Operator", 210m);
            var fourthWorker = CreateEmployee(fourthWorkerUser.Id, company.Id, "Stefan", "Baker", EmployeeRole.Worker, "Carpenter", 185m);
            var fifthWorker = CreateEmployee(fifthWorkerUser.Id, company.Id, "Ivan", "Reed", EmployeeRole.Worker, "General Worker", 165m);

            dbContext.Employees.AddRange(
                owner,
                firstTechnicalManager,
                secondTechnicalManager,
                firstForeman,
                secondForeman,
                firstWorker,
                secondWorker,
                thirdWorker,
                fourthWorker,
                fifthWorker);
            await dbContext.SaveChangesAsync();

            var concreteCrew = new Crew
            {
                CompanyId = company.Id,
                Name = "Concrete Crew",
                TechnicalManagerId = firstTechnicalManager.Id,
                ForemanId = firstForeman.Id
            };
            var structuralCrew = new Crew
            {
                CompanyId = company.Id,
                Name = "Structural Crew",
                TechnicalManagerId = secondTechnicalManager.Id,
                ForemanId = secondForeman.Id
            };

            dbContext.Crews.AddRange(concreteCrew, structuralCrew);
            await dbContext.SaveChangesAsync();

            firstWorker.CrewId = concreteCrew.Id;
            secondWorker.CrewId = concreteCrew.Id;
            thirdWorker.CrewId = concreteCrew.Id;
            fourthWorker.CrewId = structuralCrew.Id;
            fifthWorker.CrewId = structuralCrew.Id;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var activeProject = new Project
            {
                CompanyId = company.Id,
                TechnicalManagerId = firstTechnicalManager.Id,
                Name = "Riverside Residences",
                ClientName = "Riverside Development Group",
                Address = "42 River Road, Sofia",
                Status = ProjectStatus.Active,
                StartDate = today.AddDays(-45)
            };
            var planningProject = new Project
            {
                CompanyId = company.Id,
                TechnicalManagerId = secondTechnicalManager.Id,
                Name = "North Point Warehouse",
                ClientName = "North Point Logistics",
                Address = "18 Industrial Park, Sofia",
                Status = ProjectStatus.Planning,
                StartDate = today.AddDays(14),
                EndDate = today.AddDays(180)
            };

            dbContext.Projects.AddRange(activeProject, planningProject);
            await dbContext.SaveChangesAsync();

            dbContext.ProjectCrews.AddRange(
                new ProjectCrew
                {
                    ProjectId = activeProject.Id,
                    CrewId = concreteCrew.Id,
                    AssignedFrom = activeProject.StartDate
                },
                new ProjectCrew
                {
                    ProjectId = activeProject.Id,
                    CrewId = structuralCrew.Id,
                    AssignedFrom = today.AddDays(-21)
                },
                new ProjectCrew
                {
                    ProjectId = planningProject.Id,
                    CrewId = structuralCrew.Id,
                    AssignedFrom = planningProject.StartDate,
                    AssignedTo = planningProject.EndDate
                });
            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    private static async Task<IdentityUser> CreateUserAsync(UserManager<IdentityUser> userManager, string email)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };
        var result = await userManager.CreateAsync(user, DevelopmentSeedConstants.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return user;
    }

    private static Employee CreateEmployee(
        string userId,
        int companyId,
        string firstName,
        string lastName,
        EmployeeRole role,
        string jobTitle,
        decimal? dailyRate)
    {
        return new Employee
        {
            UserId = userId,
            CompanyId = companyId,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            JobTitle = jobTitle,
            DailyRate = dailyRate
        };
    }
}
