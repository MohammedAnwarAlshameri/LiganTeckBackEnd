using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LijanTeck.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) => _db = db;

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
        {
            var start = from ?? DateTime.UtcNow.AddDays(-30);
            var end = to ?? DateTime.UtcNow;

            var activeSubs = await _db.Set<Subscription>()
                .CountAsync(s => !s.IsDeleted && s.SubStatusid == 1
                                 && s.StartDateUtc <= end
                                 && (s.EndDateUtc == null || s.EndDateUtc >= start), ct);

            var paidTotals = await _db.Set<Invoice>()
                .Where(i => !i.IsDeleted && i.InvoiceStatusid == 2
                            && i.PaidAtUtc >= start && i.PaidAtUtc <= end)
                .Select(i => i.AmountTotal)
                .ToListAsync(ct);

            var ticketsOpen = await _db.Set<Ticket>().CountAsync(t => !t.IsDeleted && t.TicketStatusid == 1, ct);
            var ticketsClosed = await _db.Set<Ticket>().CountAsync(t => !t.IsDeleted && t.TicketStatusid == 3, ct);

            return Ok(new
            {
                period = new { start, end },
                activeSubscriptions = activeSubs,
                revenue = paidTotals.Sum(),
                tickets = new { open = ticketsOpen, closed = ticketsClosed }
            });
        }

        [HttpGet("series/daily")]
        public async Task<IActionResult> Daily([FromQuery] int days = 30, CancellationToken ct = default)
        {
            days = Math.Max(1, Math.Abs(days));
            var start = DateTime.UtcNow.Date.AddDays(-days);
            var end = DateTime.UtcNow.Date.AddDays(1);

            var inv = await _db.Set<Invoice>()
                .Where(i => !i.IsDeleted && i.InvoiceStatusid == 2 && i.PaidAtUtc >= start && i.PaidAtUtc < end)
                .Select(i => new { day = i.PaidAtUtc!.Value.Date, amount = i.AmountTotal })
                .ToListAsync(ct);

            var grouped = inv.GroupBy(x => x.day)
                             .Select(g => new { day = g.Key, revenue = g.Sum(x => x.amount) })
                             .OrderBy(x => x.day)
                             .ToList();

            return Ok(grouped);
        }
    }
}
