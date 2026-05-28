using Exam.Domain.Entities.Identity;

namespace Exam.Domain.Interface.Authentication
{
    public interface IRoleManagement
    {
        Task<string?> GetUserRole(string userEmail);
        Task<bool> AddUserToRole(AppUser user, string rolename);
    }
}
