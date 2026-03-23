using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Exam.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exam.Domain.Interface.Authentication
{
    public interface IUserManagement
    {
        // Register
        Task<IdentityResult> CreateUser(AppUser user, string password);

        // Login
        Task<bool> CheckPassword(AppUser user, string password);

        // Get users
        Task<AppUser?> GetUserByEmail(string email);
        Task<AppUser?> GetUserById(int id);
        Task<IEnumerable<AppUser>> GetAllUsers();

        // Delete
        Task<bool> RemoveUserByEmail(string email);

        // Claims
        Task<List<Claim>> GetUserClaim(string email);

        // Password management
        Task<IdentityResult> ChangePassword(AppUser user, string currentPassword, string newPassword);
    }
}
