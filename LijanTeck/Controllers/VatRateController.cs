using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/admin/vatrates")]
    [Authorize(Policy = "AdminOnly")]
    public class VatRateController : Controller
    {
        private readonly IVatRateService _svc;
        public VatRateController(IVatRateService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
            => Ok(await _svc.ListAsync(ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VatRateDto dto, CancellationToken ct)
            => Ok(new { VatRateId = await _svc.CreateAsync(dto, ct) });

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] VatRateDto dto, CancellationToken ct)
        { await _svc.UpdateAsync(id, dto, ct); return Ok(); }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        { await _svc.DeleteAsync(id, ct); return Ok(); }
    }
}
