using Exam.Application.Dto.Admin;
using Exam.Application.Services.Interfaces;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Exam.Application.Services.Implementation
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardDTO> GetStatsAsync()
        {
            var students = await _unitOfWork.Repository<Student>().GetAllAsync();
            var instructors = await _unitOfWork.Repository<Instructor>().GetAllAsync();
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            var exams = await _unitOfWork.Repository<Exam.Domain.Entities.Exam>().GetAllAsync();

            return new AdminDashboardDTO
            {
                TotalStudents = students.Count(s => !s.IsDeleted),
                TotalInstructors = instructors.Count(i => !i.IsDeleted),
                TotalCourses = courses.Count(c => !c.IsDeleted),
                TotalExams = exams.Count(e => !e.IsDeleted),
                ActiveExams = exams.Count(e => e.IsPublished && !e.IsDeleted)
            };
        }
    }
}
