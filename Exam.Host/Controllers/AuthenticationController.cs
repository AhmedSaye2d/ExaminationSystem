using Exam.Application.Dto.Identity;
using Exam.Application.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        // ================= Register =================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUser user)
        {
            var res = await _authenticationService.CreateUser(user);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        // ================= Login =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            var res = await _authenticationService.LoginUser(user);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        // ================= Refresh Token =================
        // ❗ لازم AllowAnonymous
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await _authenticationService.ReviveToken(request.RefreshToken);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        // ================= Logout (حقيقي) =================
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            var res = await _authenticationService.Logout(request.RefreshToken);
            return res.Success ? Ok(res) : BadRequest(res);
        }


        // ================= Current User =================
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }
    }
}
