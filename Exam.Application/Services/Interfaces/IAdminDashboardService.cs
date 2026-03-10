using Exam.Application.Dto.Admin;
using System.Threading.Tasks;

namespace Exam.Application.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDTO> GetStatsAsync();
    }
}
