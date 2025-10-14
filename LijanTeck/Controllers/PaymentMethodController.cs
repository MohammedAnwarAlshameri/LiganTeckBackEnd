using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/admin/payment-methods")]
    [Authorize(Policy = "AdminOnly")] 
    public class PaymentMethodController : Controller
    {
        private readonly IPaymentMethodService _svc;
        public PaymentMethodController(IPaymentMethodService svc) => _svc = svc;

        [HttpGet("by-tenant/{tenantId:long}")]
        public async Task<IActionResult> ByTenant(long tenantId, CancellationToken ct)
            => Ok(await _svc.ListByTenantAsync(tenantId, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentMethodUpsertDto dto, CancellationToken ct)
            => Ok(new { PaymentMethodId = await _svc.CreateAsync(dto, ct) });

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] PaymentMethodUpsertDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return Ok(); }

        [HttpPatch("{id:long}/make-default")]
        public async Task<IActionResult> MakeDefault(long id, CancellationToken ct)
        { await _svc.MakeDefaultAsync(id, ct); return Ok(); }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> SoftDelete(long id, CancellationToken ct)
        { await _svc.SoftDeleteAsync(id, ct); return Ok(); }
    }
}
