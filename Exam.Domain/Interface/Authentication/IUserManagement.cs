using System.Security.Claims;
using Exam.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exam.Domain.Interface.Authentication
{
    public interface IUserManagement
    {
        // Register
        Task<bool> CreateUser(AppUser user, string password);

        // Login
        Task<bool> CheckPassword(AppUser user, string password);

        // Get users
        Task<AppUser?> GetUserByEmail(string email);
        Task<AppUser?> GetUserById(string id);
        Task<IEnumerable<AppUser>> GetAllUsers();

        // Delete
        Task<bool> RemoveUserByEmail(string email);

        // Claims
        Task<List<Claim>> GetUserClaim(string email);
    }
}
