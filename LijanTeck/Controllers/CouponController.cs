using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/admin/coupons")]
    [Authorize(Policy = "AdminOnly")]
    public class CouponController : Controller
    {
        private readonly ICouponService _svc;
        public CouponController(ICouponService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool includeDeleted = false, [FromQuery] bool? active = null, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(includeDeleted, active, ct));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id, CancellationToken ct)
            => (await _svc.GetAsync(id, ct)) is { } vm ? Ok(vm) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CouponUpsertDto dto, CancellationToken ct)
            => Ok(new { CouponId = await _svc.CreateAsync(dto, ct) });

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] CouponUpsertDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return Ok(); }

        [HttpPatch("{id:long}/activate")]
        public async Task<IActionResult> Activate(long id, [FromQuery] bool active = true, CancellationToken ct = default)
        { await _svc.ActivateAsync(id, active, ct); return Ok(); }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> SoftDelete(long id, CancellationToken ct)
        { await _svc.SoftDeleteAsync(id, ct); return Ok(); }
    }
}
