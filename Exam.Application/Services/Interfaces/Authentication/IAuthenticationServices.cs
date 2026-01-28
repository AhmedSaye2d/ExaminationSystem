using Exam.Application.Dto.Common;
using Exam.Application.Dto.Identity;
namespace Exam.Application.Services.Interfaces.Authentication
{
    public interface IAuthenticationServices
    {
        Task<ServiceResponse> CreateUser(CreateUser user);
        Task<LoginResponse> LoginUser(Login user);
        Task<LoginResponse> ReviveToken(string refreshToken);

    }
}
