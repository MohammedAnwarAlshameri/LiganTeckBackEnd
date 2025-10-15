using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/admin/tenants")]
    //[Authorize(Policy = "AdminOnly")]
    public class TenantController : Controller
    {
        private readonly ITenantService _svc;

        public TenantController(ITenantService svc)
            => _svc = svc;

        // GET /api/admin/tenants/search?q=...&page=1&pageSize=10&statusId=1
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<TenantListItemDto>>> Search(
            [FromQuery] string? q,
            [FromQuery] long? statusId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var res = await _svc.SearchAsync(q, statusId, page, pageSize, ct);
            return Ok(res);
        }

        // GET /api/admin/tenants/recent?take=5
        [HttpGet("recent")]
        public async Task<ActionResult<List<TenantListItemDto>>> Recent([FromQuery] int take = 5, CancellationToken ct = default)
            => Ok(await _svc.RecentAsync(Math.Clamp(take, 1, 50), ct));

        // GET /api/admin/tenants/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<TenantListItemDto>> Get(long id, CancellationToken ct)
        {
            var dto = await _svc.GetByIdAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // DELETE /api/admin/tenants/{id}  (Soft delete)
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            // لو عندك خدمة CurrentUser استعملها لجلب الـ UserId
            long? byUser = null; // ضع Id المستخدم الحالي لو متاح
            var ok = await _svc.SoftDeleteAsync(id, byUser, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
