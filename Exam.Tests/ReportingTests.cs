using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Exam.Infrastructure.Data;
using Exam.Infrastructure.Repository;
using Exam.Domain.Entities;
using Exam.Application.Services.Implementation;
using AutoMapper;
using Moq;
using ClosedXML.Excel;
using System.IO;

namespace Exam.Tests
{
    public class ReportingTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task TestExcelGeneration_ConsolidatedReport_ThrottlingAndData()
        {
            var dbContext = GetDbContext();
            
            var exam = new Exam.Domain.Entities.Exam { Id = 1, Name = "Test Exam" };
            dbContext.Exams.Add(exam);
            
            var student1 = new Student { Id = 1, FirstName = "Ahmed", LastName = "Mohamed" };
            var student2 = new Student { Id = 2, FirstName = "Omar", LastName = "Ali" };
            dbContext.Students.AddRange(student1, student2);
            
            var examStudent1 = new ExamStudent { Id = 1, ExamId = 1, StudentId = 1, StartDate = new DateTime(2026, 5, 4, 10, 0, 0), EndDate = new DateTime(2026, 5, 4, 11, 0, 0) };
            var examStudent2 = new ExamStudent { Id = 2, ExamId = 1, StudentId = 2, StartDate = new DateTime(2026, 5, 4, 10, 0, 0), EndDate = new DateTime(2026, 5, 4, 11, 0, 0) };
            dbContext.ExamStudents.AddRange(examStudent1, examStudent2);

            var logs = new[]
            {
                new ProctoringLog { Id = 1, ExamId = 1, StudentId = 1, Cheating = true, CurrentEvent = "phone_detected", RiskLevel = "HIGH", Timestamp = new DateTime(2026, 5, 4, 10, 5, 0), EvidenceImagePath = "evidence/exam_1/student_1/phone.jpg" },
                new ProctoringLog { Id = 2, ExamId = 1, StudentId = 1, Cheating = true, CurrentEvent = "phone_detected", RiskLevel = "HIGH", Timestamp = new DateTime(2026, 5, 4, 10, 5, 2), EvidenceImagePath = "evidence/exam_1/student_1/phone2.jpg" }, 
                new ProctoringLog { Id = 3, ExamId = 1, StudentId = 1, Cheating = true, CurrentEvent = "no_face", RiskLevel = "VERY HIGH", Timestamp = new DateTime(2026, 5, 4, 10, 10, 0), EvidenceImagePath = "evidence/exam_1/student_1/noface.jpg" },
                new ProctoringLog { Id = 4, ExamId = 1, StudentId = 2, Cheating = false, CurrentEvent = "ok", RiskLevel = "LOW", Timestamp = new DateTime(2026, 5, 4, 10, 15, 0) }
            };
            dbContext.ProctoringLogs.AddRange(logs);
            await dbContext.SaveChangesAsync();

            var unitOfWork = new UnitOfWork(dbContext);
            var mapperMock = new Mock<IMapper>();
            
            var reportingService = new ReportingService(unitOfWork, mapperMock.Object);

            var excelBytes = await reportingService.GetReportsExcelByExamIdAsync(1);

            Assert.NotNull(excelBytes);
            Assert.True(excelBytes.Length > 0);

            using var stream = new MemoryStream(excelBytes);
            using var workbook = new XLWorkbook(stream);
            
            Assert.Single(workbook.Worksheets);
            var worksheet = workbook.Worksheets.First();
            Assert.Equal("Exam Report", worksheet.Name);

            Assert.Equal("Exam ID", worksheet.Cell(1, 1).GetString());
            Assert.Equal("1", worksheet.Cell(1, 2).GetString());
            Assert.Equal("Total Students", worksheet.Cell(2, 1).GetString());
            Assert.Equal("2", worksheet.Cell(2, 2).GetString());
            Assert.Equal("Total Students With Violations", worksheet.Cell(3, 1).GetString());
            Assert.Equal("1", worksheet.Cell(3, 2).GetString());

            Assert.Equal("Student Name", worksheet.Cell(6, 1).GetString());
            
            Assert.Equal("Ahmed Mohamed", worksheet.Cell(7, 1).GetString());
            Assert.Equal("1", worksheet.Cell(7, 2).GetString());
            Assert.Equal("1", worksheet.Cell(7, 3).GetString());
            Assert.Equal("Phone", worksheet.Cell(7, 4).GetString()); 
            Assert.Equal("evidence/exam_1/student_1/phone.jpg", worksheet.Cell(7, 9).GetString());

            Assert.Equal("Ahmed Mohamed", worksheet.Cell(8, 1).GetString());
            Assert.Equal("Phone", worksheet.Cell(8, 4).GetString()); 

            var rowCount = worksheet.LastRowUsed().RowNumber();
            Assert.Equal(9, rowCount);
        }
    }
}
