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
    // [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _svc;
        private readonly ApplicationDbContext _db;

        public TicketsController(ITicketService svc, ApplicationDbContext db)
        {
            _svc = svc; _db = db;
        }

        // إنشاء تذكرة
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TicketCreateRequest req, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(req, ct);
            return Ok(new { TicketId = id });
        }

        // رد قديم (ما زال يعمل)
        [HttpPost("{id:long}/reply")]
        public async Task<IActionResult> Reply(long id, [FromBody] TicketReplyRequest req, CancellationToken ct)
        {
            if (req.TicketId != id) return BadRequest(new { error = "TicketId mismatch." });

            var exists = await _db.Set<Ticket>().AnyAsync(t => t.TicketId == id && !t.IsDeleted, ct);
            if (!exists) return NotFound();

            await _svc.AddReplyAsync(req, ct);
            return Ok();
        }

        // جلب محادثة التذكرة (للعرض داخل تفاصيل التذكرة)
        [HttpGet("{ticketId:long}/chat")]
        public async Task<IActionResult> GetChat(long ticketId, CancellationToken ct)
        {
            var exists = await _db.Set<Ticket>().AnyAsync(t => t.TicketId == ticketId && !t.IsDeleted, ct);
            if (!exists) return NotFound();

            var list = await _db.Set<TicketChat>()
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.ChatId)
                .Select(c => new
                {
                    c.ChatId,
                    c.TicketId,
                    c.TenantId,
                    c.TenantText,
                    c.AdminText,
                    c.TenantTextAtUtc,
                    c.AdminTextAtUtc,
                    c.ChatLevel
                })
                .ToListAsync(ct);

            return Ok(list);
        }

        // المحادثة الجديدة الموحدة (عميل/إدارة)
        [HttpPost("{ticketId:long}/chat")]
      //  [Authorize] // يمكن تخصيصها CustomerOnly/AdminsOnly حسب الحاجة
        public async Task<IActionResult> AddChat(long ticketId, [FromBody] AddChatRequest request, CancellationToken ct)
        {
            var ok = await _svc.AddChatAsync(ticketId, request.TenantId, request.Message, request.IsAdmin, ct);
            return ok ? Ok() : NotFound();
        }

        // جلب بسيط حسب المستأجر (للعميل)
        [HttpGet("user/{tenantId:long}")]
       // [Authorize]
        public async Task<IActionResult> GetUserTickets(long tenantId, CancellationToken ct)
            => Ok(await _svc.GetUserTicketsAsync(tenantId, ct));

        // نسخة سريعة قديمة
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

        // لوحة الموظف (بحث/فلترة/صفحة)
        [HttpGet("admin/all")]
       // [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] string? q,
            [FromQuery] string? tenantId,
            [FromQuery] int? statusId,
            [FromQuery] string? priority,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var res = await _svc.GetAllTicketsAsync(q, tenantId, statusId, priority, page, pageSize, ct);
            return Ok(res);
        }

        // تحديث الحالة
        [HttpPut("{id:long}/status")]
       // [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        {
            var ok = await _svc.UpdateStatusAsync(id, request.StatusId, ct);
            return ok ? NoContent() : NotFound();
        }

        // تحديث الأولوية
        [HttpPut("{id:long}/priority")]
       // [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> UpdatePriority(long id, [FromBody] UpdatePriorityRequest request, CancellationToken ct)
        {
            var ok = await _svc.UpdatePriorityAsync(id, request.Priority, ct);
            return ok ? NoContent() : NotFound();
        }

        // عمليات جماعية
        [HttpPut("bulk/status")]
      //  [Authorize(Policy = "AdminsOnly")]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkUpdateStatusRequest request, CancellationToken ct)
        {
            var ok = await _svc.BulkUpdateStatusAsync(request.TicketIds, request.StatusId, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
