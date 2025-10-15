using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto req, CancellationToken ct)
            => Ok(await _auth.RegisterAsync(req, ct));

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto req, CancellationToken ct)
        {
            try { return Ok(await _auth.LoginAsync(req, ct)); }
            catch (InvalidOperationException) { return Unauthorized(new { message = "Invalid credentials." }); }
        }
    }
}
