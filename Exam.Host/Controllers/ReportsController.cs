using System;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Exam.Domain.Constants;

namespace Exam.Host.Controllers
{
    // Only instructors and admins can view all reports and export Excel for an exam
    [Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("exam/{examId}")]
        public async Task<IActionResult> GetReports(int examId)
        {
            var reports = await _reportingService.GetSessionReportsByExamIdAsync(examId);
            return Ok(reports);
        }

        // Students can view only their own TXT report; instructors/admins can view any student's TXT report
        [HttpGet("exam/{examId}/student/{studentId}/report/txt")]
        [Authorize]
        public async Task<IActionResult> GetStudentSessionReportTxt(int examId, int studentId)
        {
            // If the user is a student, ensure they are requesting their own report
            if (User.IsInRole(AppRoles.Student))
            {
                var userId = GetUserId();
                if (!userId.HasValue || userId.Value != studentId)
                {
                    return Forbid();
                }
            }
            var reportContent = await _reportingService.GetStudentSessionReportTxtAsync(examId, studentId);
            if (reportContent == null) return NotFound();
            return Content(reportContent, "text/plain");
        }

        // Excel export for the whole exam: only instructors/admins
        [HttpGet("exam/{examId}/excel")]
        [Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
        public async Task<IActionResult> ExportToExcel(int examId)
        {
            var content = await _reportingService.GetReportsExcelByExamIdAsync(examId);
            if (content == null) return NotFound();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Proctoring_Report_Exam_{examId}.xlsx");
        }

        // Student Excel report: students can get their own, instructors/admins any
        [HttpGet("exam/{examId}/student/{studentId}/report/excel")]
        [Authorize]
        public async Task<IActionResult> ExportStudentSessionToExcel(int examId, int studentId)
        {
            if (User.IsInRole(AppRoles.Student))
            {
                var userId = GetUserId();
                if (!userId.HasValue || userId.Value != studentId)
                {
                    return Forbid();
                }
            }
            var content = await _reportingService.GetStudentSessionReportExcelAsync(examId, studentId);
            if (content == null) return NotFound();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Proctoring_Report_Exam_{examId}_Student_{studentId}.xlsx");
        }
        // Helper to extract authenticated user id (supports "sub" or "id" claim)
        private int? GetUserId()
        {
            var claim = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (int.TryParse(claim, out var uid))
                return uid;
            return null;
        }
    }
}
