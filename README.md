# Builder Company Manager

Builder Company Manager is an ASP.NET Core MVC application for managing the day-to-day operations of a construction company. It brings employees, crews, construction projects, working hours, payroll summaries, and management activity into one company-scoped workspace.

The application is designed around a company hierarchy:

- **Owner** manages the company, employees, crews, projects, attendance, payroll, and activity log.
- **Technical Manager** works with the projects and crews assigned to them.
- **Foreman** manages attendance for their own crew.
- **Worker** can review their own monthly working-hours calendar and earnings summary.

## Features

- Company-first registration that creates the company and its owner account in one transaction.
- Company-scoped employee management with role, job title, daily-rate, account-status, and password-reset controls.
- Crew and project management, including time-bound crew-to-project assignments.
- Role-aware attendance management with working hours, overtime, bonuses, and daily comments.
- Monthly employee calendars and earnings calculated from attendance rate snapshots.
- Owner payroll report with month, crew, and project filters.
- Owner activity log for important management actions.
- Development demo data for a complete sample company with ten users, crews, and projects.

## Technology

- .NET 9 and ASP.NET Core MVC
- Entity Framework Core 9
- ASP.NET Core Identity
- Microsoft SQL Server / SQL Server Express
- Razor Views and CSS

## Prerequisites

Install the following before running the project:

- Visual Studio 2022 with the **ASP.NET and web development** workload, or the .NET 9 SDK.
- SQL Server Express, SQL Server LocalDB, or another SQL Server instance.
- SQL Server Management Studio is recommended for inspecting the database, but it is not required.

## Quick Start

### 1. Clone and restore

```powershell
git clone https://github.com/Bankeli/BuildingCompanyManager.git
cd BuildingCompanyManager
dotnet restore
```

Open `BuildingCompanyManager.sln` in Visual Studio after restoring the packages.

### 2. Configure SQL Server

Copy the tracked template to the ignored local file:

```powershell
Copy-Item BuildingCompanyManager/appsettings.Development.example.json BuildingCompanyManager/appsettings.Development.json
```

Then update `BuildingCompanyManager/appsettings.Development.json` with your SQL Server instance, database name, and preferred demo-data setting:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=BuilderManager;Trusted_Connection=True;Encrypt=False;"
  },
  "DemoData": {
    "SeedOnStartup": false
  }
}
```

Choose the server name that works in SQL Server Management Studio:

| Installation | Server value |
| --- | --- |
| Default SQL Server instance | `.` |
| SQL Server Express named instance | `.\\SQLEXPRESS` |
| SQL Server LocalDB | `(localdb)\\MSSQLLocalDB` |

### 3. Start the application

Start the application with `F5` or `Ctrl+F5` in Visual Studio, or run:

```powershell
dotnet run --project BuildingCompanyManager
```

When the application starts, it automatically applies the Entity Framework Core migrations. If the configured database does not exist, SQL Server creates it. If it already exists, the application uses it and applies only migrations that have not been applied yet.

You can still run migrations manually from Visual Studio when needed:

```powershell
Update-Database
```

The first public account must be created through **Register company**. This creates a Company, an ASP.NET Core Identity user, and an Owner employee profile.

## Development Demo Data

The application includes a development-only seed for a sample company named **BuildPro Demo Ltd**. It creates:

- 1 Owner
- 2 Technical Managers
- 2 Foremen
- 5 Workers
- 2 Crews
- 1 active project and 1 planned project
- Crew-to-project assignments

It does not create attendance records. Use the Attendance area to add working hours while testing the role-specific workflows.

### Enable the seed

1. Use a new empty database, for example `BuilderManagerDemo`.
2. Copy `BuildingCompanyManager/appsettings.Development.example.json` to `BuildingCompanyManager/appsettings.Development.json`.
3. Update the SQL Server instance in the copied file if needed. The template already enables `SeedOnStartup`:

```json
{
  "DemoData": {
    "SeedOnStartup": true
  }
}
```

4. Start the application once. The demo company and accounts are created automatically.
5. Set `SeedOnStartup` back to `false` after the data has been created.

The seeder is idempotent: it does nothing when **BuildPro Demo Ltd** already exists. To start over, create a new empty demo database or remove the old demo database first. Do not use the demo accounts or seed flag in a production environment.

## Demo Accounts

All demo accounts use the following password:

```text
BuilderDemo!2026
```

| Role | Name | Email |
| --- | --- | --- |
| Owner | Oliver Grant | `owner@buildpro-demo.local` |
| Technical Manager | Thomas Hale | `thomas.hale@buildpro-demo.local` |
| Technical Manager | Elena Stone | `elena.stone@buildpro-demo.local` |
| Foreman | Peter Mills | `peter.mills@buildpro-demo.local` |
| Foreman | Maria Woods | `maria.woods@buildpro-demo.local` |
| Worker | George Ross | `george.ross@buildpro-demo.local` |
| Worker | Daniel Price | `daniel.price@buildpro-demo.local` |
| Worker | Victor King | `victor.king@buildpro-demo.local` |
| Worker | Stefan Baker | `stefan.baker@buildpro-demo.local` |
| Worker | Ivan Reed | `ivan.reed@buildpro-demo.local` |

## Project Structure

```text
BuildingCompanyManager.sln
|
|-- BuildingCompanyManager.Data/
|   |-- Models/              Database entities
|   |-- Enums/               Application enums
|   `-- Common/              Shared database constants
|
`-- BuildingCompanyManager/
    |-- Controllers/         MVC controllers
    |-- Models/              View and input models
    |-- Views/               Razor views
    |-- Services/            Cross-cutting application services
    |-- Data/Migrations/     Entity Framework Core migrations
    `-- Data/Seed/           Development demo-data seed
```

## Database Migrations

Every database schema change is represented by an Entity Framework Core migration. To create a migration after changing entity models, use:

```powershell
Add-Migration DescriptiveMigrationName
Update-Database
```

## For Contributors

- Keep local connection strings and other environment-specific settings out of source control.
- Demo credentials are intentionally public only for local development and testing.
