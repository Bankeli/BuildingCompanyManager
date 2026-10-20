using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingCompanyManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillLegacyAttendanceHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [AttendanceRecords]
                SET [WorkedHours] = CASE
                        WHEN [IsPresent] = 1 AND [ExtraHours] > 16 THEN 24
                        WHEN [IsPresent] = 1 THEN 8 + [ExtraHours]
                        ELSE [ExtraHours]
                    END,
                    [IsPresent] = CASE
                        WHEN [IsPresent] = 1 OR [ExtraHours] > 0 THEN CAST(1 AS bit)
                        ELSE CAST(0 AS bit)
                    END
                WHERE [WorkedHours] = 0
                    AND ([IsPresent] = 1 OR [ExtraHours] > 0);
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AttendanceRecords_ExtraHoursWithinWorkedHours",
                table: "AttendanceRecords",
                sql: "[ExtraHours] <= [WorkedHours]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AttendanceRecords_ExtraHoursWithinWorkedHours",
                table: "AttendanceRecords");
        }
    }
}
