using AutoMapper;
using Exam.Application.Dto.Common;
using Exam.Application.Dto.Identity;
using Exam.Application.Services.Interfaces.Authentication;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Interface.Authentication;
using FluentValidation;
using System.Linq;
using System.Threading.Tasks;

namespace Exam.Application.Services.Implementation
{
    public class AuthenticationService : IAuthenticationServices
    {
        private readonly IUserManagement _userManagement;
        private readonly ITokenManagement _tokenManagement;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateUser> _createUserValidator;
        private readonly IValidator<Login> _loginValidator;

        public AuthenticationService(
            IUserManagement userManagement,
            ITokenManagement tokenManagement,
            IMapper mapper,
            IValidator<CreateUser> createUserValidator,
            IValidator<Login> loginValidator)
        {
            _userManagement = userManagement;
            _tokenManagement = tokenManagement;
            _mapper = mapper;
            _createUserValidator = createUserValidator;
            _loginValidator = loginValidator;
        }

        // ✅ Register
        public async Task<ServiceResponse> CreateUser(CreateUser userDto)
        {
            var validation = await _createUserValidator.ValidateAsync(userDto);
            if (!validation.IsValid)
                return ServiceResponse.Fail(validation.Errors.First().ErrorMessage);

            var appUser = _mapper.Map<AppUser>(userDto);

            bool created = await _userManagement.CreateUser(appUser, userDto.Password);
            if (!created)
                return ServiceResponse.Fail("User already exists");

            return ServiceResponse.Ok("User created successfully");
        }

        // ✅ Login
        public async Task<LoginResponse> LoginUser(Login loginDto)
        {
            var validation = await _loginValidator.ValidateAsync(loginDto);
            if (!validation.IsValid)
                return new LoginResponse(false, validation.Errors.First().ErrorMessage);

            var appUser = await _userManagement.GetUserByEmail(loginDto.Email);
            if (appUser == null)
                return new LoginResponse(false, "Invalid email or password");

            bool validPassword =
                await _userManagement.CheckPassword(appUser, loginDto.Password);

            if (!validPassword)
                return new LoginResponse(false, "Invalid email or password");

            var claims = await _userManagement.GetUserClaim(appUser.Email!);

            string token = _tokenManagement.GenerateToken(claims);
            string refreshToken = _tokenManagement.GetRefreshToken();

            await _tokenManagement.UpdateRefreshToken(appUser.Id, refreshToken);

            return new LoginResponse(true, "Login successful", token, refreshToken);
        }

        // ✅ Refresh Token
        public async Task<LoginResponse> ReviveToken(string refreshToken)
        {
            bool valid = await _tokenManagement.ValidateRefreshToken(refreshToken);
            if (!valid)
                return new LoginResponse(false, "Invalid refresh token");

            string? userId = await _tokenManagement.GetUserIdByRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(userId))
                return new LoginResponse(false, "User not found");

            var appUser = await _userManagement.GetUserById(userId);
            if (appUser == null)
                return new LoginResponse(false, "User not found");

            var claims = await _userManagement.GetUserClaim(appUser.Email!);

            string newToken = _tokenManagement.GenerateToken(claims);
            string newRefreshToken = _tokenManagement.GetRefreshToken();

            await _tokenManagement.UpdateRefreshToken(appUser.Id, newRefreshToken);

            return new LoginResponse(true, "Token refreshed", newToken, newRefreshToken);
        }
    }
}
