using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/plans")]
   // [Authorize]
    public class PlansController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public PlansController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var plans = await _db.Set<Plan>()
                .Where(p => p.IsActive && !p.IsDeleted)
                .Select(p => new { p.PlanId, p.PlanNameAr, p.PlanNameEn, p.MonthlyPrice })
                .ToListAsync(ct);

            return Ok(plans);
        }
    }
}
