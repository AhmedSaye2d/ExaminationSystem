using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserRelationshipsComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_ExamAttempts_AspNetUsers_StudentId",
                table: "ExamAttempts",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_AspNetUsers_InstructorId",
                table: "Exams",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamAttempts_AspNetUsers_StudentId",
                table: "ExamAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_AspNetUsers_InstructorId",
                table: "Exams");
        }
    }
}
