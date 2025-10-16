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
        private readonly ILogger<TenantController> _logger;

        public TenantController(ITenantService svc, ILogger<TenantController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // GET /api/admin/tenants/search?q=...&page=1&pageSize=10&statusId=1
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<TenantListItemDto>>> Search(
            [FromQuery] string? q,
            [FromQuery] long? statusId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 200);

                _logger.LogInformation("Searching tenants with q={Query}, statusId={StatusId}, page={Page}, pageSize={PageSize}",
                    q, statusId, page, pageSize);

                var res = await _svc.SearchAsync(q, statusId, page, pageSize, ct);

                _logger.LogInformation("Found {Count} tenants out of {Total}", res.Items.Count, res.Total);

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching tenants. Query: {Query}, StatusId: {StatusId}, Page: {Page}, PageSize: {PageSize}",
                    q, statusId, page, pageSize);

                return StatusCode(500, new
                {
                    message = "An error occurred while searching tenants",
                    error = ex.Message
                });
            }
        }

        // GET /api/admin/tenants/recent?take=5
        [HttpGet("recent")]
        public async Task<ActionResult<List<TenantListItemDto>>> Recent([FromQuery] int take = 5, CancellationToken ct = default)
        {
            try
            {
                take = Math.Clamp(take, 1, 50);
                var result = await _svc.RecentAsync(take, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recent tenants. Take: {Take}", take);
                return StatusCode(500, new
                {
                    message = "An error occurred while getting recent tenants",
                    error = ex.Message
                });
            }
        }

        // GET /api/admin/tenants/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<TenantListItemDto>> Get(long id, CancellationToken ct)
        {
            try
            {
                var dto = await _svc.GetByIdAsync(id, ct);
                return dto is null ? NotFound() : Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tenant by id. Id: {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while getting tenant",
                    error = ex.Message
                });
            }
        }

        // DELETE /api/admin/tenants/{id}  (Soft delete)
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            try
            {
                // لو عندك خدمة CurrentUser استعملها لجلب الـ UserId
                long? byUser = null; // ضع Id المستخدم الحالي لو متاح
                var ok = await _svc.SoftDeleteAsync(id, byUser, ct);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tenant. Id: {Id}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting tenant",
                    error = ex.Message
                });
            }
        }
    }
}