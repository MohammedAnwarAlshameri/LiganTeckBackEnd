using Application.DTOs;
using Application.IServices;
using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _service;
        private readonly ApplicationDbContext _db;

        public SubscriptionsController(ISubscriptionService service, ApplicationDbContext db)
        {
            _service = service;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest req, CancellationToken ct)
        {
            var dto = await _service.CreateAsync(req, ct);
            return Ok(dto);
        }

        [HttpPost("{id:long}/renew")]
        public async Task<IActionResult> Renew([FromRoute] long id, CancellationToken ct)
        {
            var ok = await _service.RenewAsync(id, ct);
            return ok ? Ok() : NotFound();
        }

        [HttpGet("by-tenant/{tenantId:long}")]
        public async Task<IActionResult> ByTenant(long tenantId, CancellationToken ct)
        {
            var list = await _db.Set<Subscription>()
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .OrderByDescending(s => s.SubscriptionId)
                .Select(s => new {
                    s.SubscriptionId,
                    s.PlanId,
                    s.MonthsCount,
                    s.AutoRenew,
                    s.SubStatusid,
                    s.StartDateUtc,
                    s.EndDateUtc,
                    s.CouponId
                })
                .ToListAsync(ct);

            return Ok(list);
        }
        // ====== إدارة ======
        [HttpGet("admin/search")]
      //  [Authorize(Policy = "AdminsOnly")]
        public async Task<ActionResult<PagedResult<SubscriptionListItemDto>>> AdminSearch(
            [FromQuery] long? tenantId,
            [FromQuery] long? planId,
            [FromQuery] int? statusId,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var res = await _service.AdminSearchAsync(tenantId, planId, statusId, fromUtc, toUtc, page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("admin/all")]
       // [Authorize(Policy = "AdminsOnly")]
        public async Task<ActionResult<PagedResult<SubscriptionListItemDto>>> AdminAll(CancellationToken ct)
            => Ok(await _service.AdminSearchAsync(null, null, null, null, null, 1, 50, ct));
    }
}
