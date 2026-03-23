using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteSystemGaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamStudents_IsSubmitted",
                table: "ExamStudents");

            migrationBuilder.DropColumn(
                name: "IsSubmitted",
                table: "ExamStudents");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ExamStudents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ExamStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PassingScore",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Settings_MaxAttempts",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "ExamResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Total",
                table: "ExamResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "ExamAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "ExamAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentDate",
                table: "CourseStudents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CourseStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExamStudents_Status",
                table: "ExamStudents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId",
                table: "ExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAnswers_ExamId",
                table: "ExamAnswers",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAnswers_StudentId",
                table: "ExamAnswers",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamAnswers_AspNetUsers_StudentId",
                table: "ExamAnswers",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamAnswers_Exams_ExamId",
                table: "ExamAnswers",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_AspNetUsers_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Exams_ExamId",
                table: "ExamResults",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamAnswers_AspNetUsers_StudentId",
                table: "ExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamAnswers_Exams_ExamId",
                table: "ExamAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_AspNetUsers_StudentId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Exams_ExamId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamStudents_Status",
                table: "ExamStudents");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_ExamId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamAnswers_ExamId",
                table: "ExamAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ExamAnswers_StudentId",
                table: "ExamAnswers");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ExamStudents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExamStudents");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Settings_MaxAttempts",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CalculatedAt",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "ExamAnswers");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "ExamAnswers");

            migrationBuilder.DropColumn(
                name: "EnrollmentDate",
                table: "CourseStudents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CourseStudents");

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmitted",
                table: "ExamStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ExamStudents_IsSubmitted",
                table: "ExamStudents",
                column: "IsSubmitted");
        }
    }
}
