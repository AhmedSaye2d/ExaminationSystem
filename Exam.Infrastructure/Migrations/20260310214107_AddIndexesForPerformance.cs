using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Questions_Type",
                table: "Questions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ExamStudents_IsSubmitted",
                table: "ExamStudents",
                column: "IsSubmitted");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_StartDate_DueDate",
                table: "Exams",
                columns: new[] { "StartDate", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Choices_IsCorrectAnswer",
                table: "Choices",
                column: "IsCorrectAnswer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Type",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_ExamStudents_IsSubmitted",
                table: "ExamStudents");

            migrationBuilder.DropIndex(
                name: "IX_Exams_StartDate_DueDate",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Name",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Code",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Choices_IsCorrectAnswer",
                table: "Choices");
        }
    }
}
