using Exam.Application.Dto.Admin;

namespace Exam.Application.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDTO> GetStatsAsync();
    }
}
