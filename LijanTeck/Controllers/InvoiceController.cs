using Application.DTOs;
using Application.IServices;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/invoices")]
   // [Authorize]
    public sealed class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _svc;

        public InvoicesController(IInvoiceService svc) => _svc = svc;

        [HttpGet("{id:long}")]
        public async Task<ActionResult<InvoiceDto>> Get(long id, CancellationToken ct)
            => Ok(await _svc.GetAsync(id, ct));

        [HttpGet("by-tenant/{tenantId:long}")]
        public async Task<ActionResult<List<InvoiceDto>>> ByTenant(long tenantId, CancellationToken ct)
            => Ok(await _svc.GetByTenantAsync(tenantId, ct));

        [HttpGet("by-sub/{subscriptionId:long}")]
        public async Task<ActionResult<List<InvoiceDto>>> BySubscription(long subscriptionId, CancellationToken ct)
            => Ok(await _svc.GetBySubscriptionAsync(subscriptionId, ct));

        [HttpPost("{invoiceId:long}/pay")]
        public async Task<IActionResult> Pay(long invoiceId, [FromBody] PayInvoiceRequest req, CancellationToken ct)
        {
            var ok = await _svc.MarkPaidAsync(invoiceId, req.PaymentMethodId, req.PaidAtUtc, ct);
            return ok ? Ok() : NotFound();
        }

        [HttpPost("{invoiceId:long}/cancel")]
       // [Authorize(Policy = "AdminsOnly")] // مثلاً
        public async Task<IActionResult> Cancel(long invoiceId, CancellationToken ct)
        {
            var ok = await _svc.CancelAsync(invoiceId, ct);
            return ok ? Ok() : NotFound();
        }
        // ==================== الإدارة (Admin) ====================
        // GET /api/invoices/admin/search?tenantId=&subscriptionId=&statusId=&fromUtc=&toUtc=&page=1&pageSize=20
        [HttpGet("admin/search")]
       // [Authorize(Policy = "AdminsOnly")]
        public async Task<ActionResult<PagedResult<InvoiceDto>>> AdminSearch(
            [FromQuery] long? tenantId,
            [FromQuery] long? subscriptionId,
            [FromQuery] int? statusId,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var res = await _svc.SearchAsync(tenantId, subscriptionId, statusId, fromUtc, toUtc, page, pageSize, ct);
            return Ok(res);
        }

        // GET /api/invoices/admin/all (اختصار لأوّل 50)
        [HttpGet("admin/all")]
        //[Authorize(Policy = "AdminsOnly")]
        public async Task<ActionResult<PagedResult<InvoiceDto>>> AdminAll(CancellationToken ct)
            => Ok(await _svc.SearchAsync(null, null, null, null, null, 1, 50, ct));
    }
}
