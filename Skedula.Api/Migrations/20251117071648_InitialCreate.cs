using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Skedula.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateJoined = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxConsecutiveWorkDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "ShiftTypes",
                columns: table => new
                {
                    ShiftTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredHeadcount = table.Column<int>(type: "int", nullable: false),
                    IsRestDay = table.Column<bool>(type: "bit", nullable: false),
                    IsLeave = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTypes", x => x.ShiftTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeFatigueMetrics",
                columns: table => new
                {
                    MetricId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsecutiveWorkDays = table.Column<int>(type: "int", nullable: false),
                    TotalWorkDaysInPeriod = table.Column<int>(type: "int", nullable: false),
                    WeekendShiftsCount = table.Column<int>(type: "int", nullable: false),
                    LastCODate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCOnWeekendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FatigueScore = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeFatigueMetrics", x => x.MetricId);
                    table.ForeignKey(
                        name: "FK_EmployeeFatigueMetrics_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    LeaveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.LeaveId);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftTypeId = table.Column<int>(type: "int", nullable: false),
                    IsAutoAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedules_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedules_ShiftTypes_ShiftTypeId",
                        column: x => x.ShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "ShiftTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftRotationTrackers",
                columns: table => new
                {
                    TrackerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftTypeId = table.Column<int>(type: "int", nullable: false),
                    RotationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastAssignedEmployeeId = table.Column<int>(type: "int", nullable: true),
                    LastAssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RotationSequence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftRotationTrackers", x => x.TrackerId);
                    table.ForeignKey(
                        name: "FK_ShiftRotationTrackers_ShiftTypes_ShiftTypeId",
                        column: x => x.ShiftTypeId,
                        principalTable: "ShiftTypes",
                        principalColumn: "ShiftTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ShiftTypes",
                columns: new[] { "ShiftTypeId", "Code", "IsLeave", "IsRestDay", "Name", "Priority", "RequiredHeadcount" },
                values: new object[,]
                {
                    { 1, "A", false, false, "A Shift", 1, 2 },
                    { 2, "C", false, false, "C Shift", 2, 3 },
                    { 3, "D", false, false, "D Shift", 3, 0 },
                    { 4, "E", false, false, "E Shift", 4, 3 },
                    { 5, "CO", false, true, "Day Off", 5, 0 },
                    { 6, "EC", false, false, "EC Shift", 6, 1 },
                    { 7, "AL", true, true, "Annual Leave", 7, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeFatigueMetrics_EmployeeId",
                table: "EmployeeFatigueMetrics",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_EmployeeId_Date",
                table: "Schedules",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ShiftTypeId",
                table: "Schedules",
                column: "ShiftTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRotationTrackers_ShiftTypeId",
                table: "ShiftRotationTrackers",
                column: "ShiftTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeFatigueMetrics");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "ShiftRotationTrackers");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ShiftTypes");
        }
    }
}
