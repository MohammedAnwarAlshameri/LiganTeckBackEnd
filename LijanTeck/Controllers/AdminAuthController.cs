using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : Controller
    {
        private readonly IAdminUserService _svc;
        public AdminAuthController(IAdminUserService svc) => _svc = svc;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] AdminLoginRequest req, CancellationToken ct)
            => Ok(await _svc.LoginAsync(req, ct));
    }
}
