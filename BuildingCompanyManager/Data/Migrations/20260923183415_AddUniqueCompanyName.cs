using BuildingCompanyManager.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingCompanyManager.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260923183415_AddUniqueCompanyName")]
public partial class AddUniqueCompanyName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Companies_Name",
            table: "Companies",
            column: "Name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Companies_Name",
            table: "Companies");
    }
}
