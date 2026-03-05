using Exam.Domain.Entities.Identity;
using Exam.Domain.Interface.Authentication;
using Exam.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exam.Infrastructure.Repository.Authentication
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRoleManagement _roleManagement;
        private readonly AppDbContext _context;

        public UserManagement(
            UserManager<AppUser> userManager,
            IRoleManagement roleManagement,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManagement = roleManagement;
            _context = context;
        }

        public async Task<IdentityResult> CreateUser(AppUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var roleName = user.UserType.ToString();
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return result;
        }

        public async Task<bool> CheckPassword(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<AppUser?> GetUserByEmail(string email)
            => await _userManager.FindByEmailAsync(email);

        public async Task<AppUser?> GetUserById(int id)
            => await _userManager.FindByIdAsync(id.ToString());

        public async Task<IEnumerable<AppUser>> GetAllUsers()
            => await _context.Users.ToListAsync();

        public async Task<bool> RemoveUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<List<Claim>> GetUserClaim(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new();

            var roleName = await _roleManagement.GetUserRole(user.Email!);

            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, roleName!)
            };
        }
    }
}
