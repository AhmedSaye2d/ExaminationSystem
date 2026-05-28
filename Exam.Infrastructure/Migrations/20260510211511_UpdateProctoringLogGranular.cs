using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProctoringLogGranular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EyeStatus",
                table: "ProctoringLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FacePresent",
                table: "ProctoringLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HeadStatus",
                table: "ProctoringLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonCount",
                table: "ProctoringLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PhoneConfidence",
                table: "ProctoringLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneDetected",
                table: "ProctoringLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EyeStatus",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "FacePresent",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "HeadStatus",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "PersonCount",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "PhoneConfidence",
                table: "ProctoringLogs");

            migrationBuilder.DropColumn(
                name: "PhoneDetected",
                table: "ProctoringLogs");
        }
    }
}
