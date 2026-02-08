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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUser user)
        {
            var res = await _authenticationService.CreateUser(user);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            var res = await _authenticationService.LoginUser(user);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await _authenticationService.ReviveToken(request.RefreshToken);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            var res = await _authenticationService.Logout(request.RefreshToken);
            return res.Success ? Ok(res) : BadRequest(res);
        }
    }
}
