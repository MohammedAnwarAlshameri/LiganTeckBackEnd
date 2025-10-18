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

/*
 [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto req, CancellationToken ct)
        {
            try
            {
                var result = await _auth.RegisterAsync(req, ct);
                return Ok(result);
            }
            // التكرارات التي نحن نطلقها يدوياً من الخدمة
            catch (InvalidOperationException ex) when (IsDuplicateMessage(ex.Message, out var fieldFromMsg))
            {
                return Conflict(new { message = ex.Message, field = fieldFromMsg });
            }
            // التكرارات القادمة من قاعدة البيانات (فهرس فريد)
            catch (DbUpdateException ex) when (TryMapUniqueViolation(ex, out var fieldFromDb))
            {
                return Conflict(new { message = $"{fieldFromDb} already exists.", field = fieldFromDb });
            }
            catch (ArgumentException bad)
            {
                return BadRequest(new { message = bad.Message });
            }
        }

        private static bool IsDuplicateMessage(string msg, out string field)
        {
            var m = (msg ?? "").ToLowerInvariant();
            if (m.Contains("username")) { field = "username"; return true; }
            if (m.Contains("tenant name")) { field = "companyName"; return true; } // نطابق اسم حقل الفرونت
            if (m.Contains("email")) { field = "email"; return true; }
            field = "";
            return false;
        }

        // يلتقط أخطاء 2627/2601 ويربطها باسم الفهرس الفريد
        private static bool TryMapUniqueViolation(DbUpdateException ex, out string field)
        {
            field = "";
            var baseEx = ex?.GetBaseException() as SqlException;
            if (baseEx == null) return false;

            // 2627: Violation of UNIQUE KEY constraint
            // 2601: Cannot insert duplicate key row in object...
            if (baseEx.Number == 2627 || baseEx.Number == 2601)
            {
                var msg = baseEx.Message ?? "";
                if (msg.Contains("UX_Tenant_Email_Active")) { field = "email"; return true; }
                if (msg.Contains("UX_Tenant_Username_Active")) { field = "username"; return true; }
                if (msg.Contains("UX_Tenant_TenantName_Active")) { field = "companyName"; return true; }
            }
            return false;
        }
*/
