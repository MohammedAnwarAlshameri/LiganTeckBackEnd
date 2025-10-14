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
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _payments;
        private readonly ApplicationDbContext _db;

        public PaymentsController(IPaymentService payments, ApplicationDbContext db)
        { _payments = payments; _db = db; }

        // يستدعيه الفرونت بعد نجاح بوابة الدفع (إن ما استخدمت Webhook)
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest req, CancellationToken ct)
        {
            var ok = await _payments.ConfirmAsync(req, ct);
            return ok ? Ok() : NotFound();
        }

        // فواتير المستأجر
        [HttpGet("invoices/by-tenant/{tenantId:long}")]
        public async Task<IActionResult> InvoicesByTenant(long tenantId, CancellationToken ct)
        {
            var list = await _db.Set<Invoice>()
                .Where(i => !i.IsDeleted &&
                            _db.Set<Subscription>().Any(s => s.SubscriptionId == i.SubscriptionId &&
                                                             s.TenantId == tenantId && !s.IsDeleted))
                .OrderByDescending(i => i.InvoiceId)
                .Select(i => new {
                    i.InvoiceId,
                    i.InvoiceNumber,
                    i.CurrencyCode,
                    i.AmountSubtotal,
                    i.DiscountAmount,
                    i.TaxAmount,
                    i.AmountTotal,
                    i.InvoiceStatusid,
                    i.IssueDateUtc,
                    i.DueDateUtc,
                    i.PaidAtUtc
                })
                .ToListAsync(ct);

            return Ok(list);
        }
    }
}
