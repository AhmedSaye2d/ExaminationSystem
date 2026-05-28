using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProctoringLogDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProctoringLogs_ExamId",
                table: "ProctoringLogs");

            migrationBuilder.AlterColumn<string>(
                name: "RiskLevel",
                table: "ProctoringLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Cheating",
                table: "ProctoringLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EventsJson",
                table: "ProctoringLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SuspiciousTime",
                table: "ProctoringLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ExamProctoringSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    ExamTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinalRiskLevel = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FinalScore = table.Column<double>(type: "float", nullable: false),
                    SuspiciousTime = table.Column<double>(type: "float", nullable: false),
                    PhoneDetectionCount = table.Column<int>(type: "int", nullable: false),
                    ExtraPersonCount = table.Column<int>(type: "int", nullable: false),
                    NoFaceCount = table.Column<int>(type: "int", nullable: false),
                    HeadViolationCount = table.Column<int>(type: "int", nullable: false),
                    GazeViolationCount = table.Column<int>(type: "int", nullable: false),
                    LongGapCount = table.Column<int>(type: "int", nullable: false),
                    TotalViolations = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFlagged = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamProctoringSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamProctoringSummaries_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamProctoringSummaries_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProctoringReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalViolations = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProctoringReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProctoringReports_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProctoringReports_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringLogs_ExamId_StudentId_CreatedAt",
                table: "ProctoringLogs",
                columns: new[] { "ExamId", "StudentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringLogs_RiskLevel",
                table: "ProctoringLogs",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ExamProctoringSummaries_ExamId_StudentId",
                table: "ExamProctoringSummaries",
                columns: new[] { "ExamId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamProctoringSummaries_FinalRiskLevel",
                table: "ExamProctoringSummaries",
                column: "FinalRiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ExamProctoringSummaries_StudentId",
                table: "ExamProctoringSummaries",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringReports_ExamId",
                table: "ProctoringReports",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringReports_StudentId",
                table: "ProctoringReports",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamProctoringSummaries");

            migrationBuilder.DropTable(
                name: "ProctoringReports");

            migrationBuilder.DropIndex(
                name: "IX_ProctoringLogs_ExamId_StudentId_CreatedAt",
                table: "ProctoringLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProctoringLogs_RiskLevel",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "Cheating",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "EventsJson",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "SuspiciousTime",
                table: "ProctoringLogs");

            migrationBuilder.AlterColumn<string>(
                name: "RiskLevel",
                table: "ProctoringLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringLogs_ExamId",
                table: "ProctoringLogs",
                column: "ExamId");
        }
    }
}
