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
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _svc;
        private readonly ApplicationDbContext _db;
        public TicketsController(ITicketService svc, ApplicationDbContext db) { _svc = svc; _db = db; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TicketCreateRequest req, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(req, ct);
            return Ok(new { TicketId = id });
        }

        [HttpPost("{id:long}/reply")]
        public async Task<IActionResult> Reply(long id, [FromBody] TicketReplyRequest req, CancellationToken ct)
        {
            if (req.TicketId != id) return BadRequest(new { error = "TicketId mismatch." });

            // تأكد أن التذكرة موجودة ثم أضف الردّ بواسطة الخدمة
            var exists = await _db.Set<Ticket>().AnyAsync(t => t.TicketId == id && !t.IsDeleted, ct);
            if (!exists) return NotFound();

            await _svc.AddReplyAsync(req, ct);
            return Ok();
        }

        [HttpGet("by-tenant/{tenantId:long}")]
        public async Task<IActionResult> ByTenant(long tenantId, CancellationToken ct)
        {
            var list = await _db.Set<Ticket>()
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .OrderByDescending(t => t.TicketId)
                .Select(t => new { t.TicketId, t.SubjectLine, t.PriorityLevel, t.TicketStatusid, t.CreatedOn })
                .ToListAsync(ct);

            return Ok(list);
        }

        // محادثات المستأجر (لعدم وجود TicketId في TicketChat)
        [HttpGet("chats/by-tenant/{tenantId:long}")]
        public async Task<IActionResult> ChatsByTenant(long tenantId, CancellationToken ct)
        {
            var chats = await _db.Set<TicketChat>()
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.ChatId)
                .Select(c => new {
                    c.ChatId,
                    c.TenantId,
                    c.TenantText,
                    c.AdminText,
                    c.TenantTextAtUtc,
                    c.AdminTextAtUtc,
                    c.ChatLevel
                })
                .ToListAsync(ct);

            return Ok(chats);
        }
    }
}
