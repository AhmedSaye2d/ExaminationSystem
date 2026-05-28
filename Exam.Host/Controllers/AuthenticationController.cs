using Exam.Application.Dto.Common;
using Exam.Application.Dto.Identity;
using Exam.Application.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.Host.Controllers
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

        /// <summary>
        /// Register a new user in the system.
        /// </summary>
        /// <param name="user">User registration information.</param>
        /// <returns>Result of the registration process.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUser user)
        {
            var res = await _authenticationService.CreateUser(user);
            var response = new ApiResponse<object>(res.Success, res.Success ? 200 : 400, res.Message, null, res.Errors);
            return res.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Authenticate a user and return access and refresh tokens.
        /// </summary>
        /// <param name="user">User login credentials.</param>
        /// <returns>Authentication tokens if successful.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            var res = await _authenticationService.LoginUser(user);
            return res.Success ? Ok(res) : Unauthorized(res);
        }

        /// <summary>
        /// Refresh an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh token request.</param>
        /// <returns>New authentication tokens.</returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var res = await _authenticationService.ReviveToken(request.RefreshToken);
            return res.Success ? Ok(res) : Unauthorized(res);
        }



    }
}

