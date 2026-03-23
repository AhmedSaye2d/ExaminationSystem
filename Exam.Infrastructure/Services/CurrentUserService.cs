using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Exam.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAdmin()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.IsInRole("Admin") ?? false;
        }
    }
}
