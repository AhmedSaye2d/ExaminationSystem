using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamStudents_StudentId_ExamId",
                table: "ExamStudents");

            migrationBuilder.CreateIndex(
                name: "IX_ExamStudents_StudentId",
                table: "ExamStudents",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamStudents_StudentId",
                table: "ExamStudents");

            migrationBuilder.CreateIndex(
                name: "IX_ExamStudents_StudentId_ExamId",
                table: "ExamStudents",
                columns: new[] { "StudentId", "ExamId" },
                unique: true);
        }
    }
}
