using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResolvePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "Exams");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAnswers_ExamStudentId_QuestionId",
                table: "ExamAnswers",
                columns: new[] { "ExamStudentId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamAnswers_ExamStudentId_QuestionId",
                table: "ExamAnswers");

            migrationBuilder.AddColumn<int>(
                name: "TotalPoints",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
