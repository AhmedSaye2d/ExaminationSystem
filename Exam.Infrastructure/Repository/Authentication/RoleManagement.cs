using Exam.Domain.Entities.Identity;
using Exam.Domain.Interface.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Exam.Infrastructure.Repository.Authentication
{
    public class RoleManagement(UserManager<AppUser> userManager) : IRoleManagement
    {
        public async Task<bool> AddUserToRole(AppUser user, string rolename)
          => (await userManager.AddToRoleAsync(user, rolename)).Succeeded;

        public async Task<string?> GetUserRole(string userEmail)
        {
            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null) return null;
            return (await userManager.GetRolesAsync(user)).FirstOrDefault();
        }
    }
}
