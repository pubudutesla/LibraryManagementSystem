using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.Authenticate(request.Username, request.Password);
            if (token == null) return Unauthorized("Invalid username or password");

            return Ok(new { Token = token });
        }
    }
}