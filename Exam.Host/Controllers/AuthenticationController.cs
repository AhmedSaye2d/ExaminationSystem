using Exam.Application.Dto.Identity;
using Exam.Application.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices _authenticationService;

        public AuthenticationController(IAuthenticationServices authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // ✅ Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUser user)
        {
            var res = await _authenticationService.CreateUser(user);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        // ✅ Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            var res = await _authenticationService.LoginUser(user);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        // ✅ Refresh Token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await _authenticationService.ReviveToken(request.RefreshToken);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        // ✅ Logout (Invalidate Refresh Token)
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _authenticationService.ReviveToken(request.RefreshToken);
            return Ok("Logged out successfully");
        }

        // ✅ Get Current Logged User
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var user = new
            {
                Id = User.FindFirst("uid")?.Value,
                Email = User.Identity?.Name,
                Role = User.FindFirst("role")?.Value,
                UserType = User.FindFirst("userType")?.Value
            };

            return Ok(user);
        }
    }
}
