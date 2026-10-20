using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingCompanyManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceWorkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Employees_MarkedByForemanId",
                table: "AttendanceRecords");

            migrationBuilder.RenameColumn(
                name: "MarkedByForemanId",
                table: "AttendanceRecords",
                newName: "RecordedByEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_MarkedByForemanId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_RecordedByEmployeeId");

            migrationBuilder.AddColumn<decimal>(
                name: "WorkedHours",
                table: "AttendanceRecords",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

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
                    END;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AttendanceRecords_WorkedHours",
                table: "AttendanceRecords",
                sql: "[WorkedHours] >= 0 AND [WorkedHours] <= 24");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_RecordedByEmployeeId",
                table: "AttendanceRecords",
                column: "RecordedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Employees_RecordedByEmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AttendanceRecords_WorkedHours",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WorkedHours",
                table: "AttendanceRecords");

            migrationBuilder.RenameColumn(
                name: "RecordedByEmployeeId",
                table: "AttendanceRecords",
                newName: "MarkedByForemanId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_RecordedByEmployeeId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_MarkedByForemanId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_MarkedByForemanId",
                table: "AttendanceRecords",
                column: "MarkedByForemanId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
