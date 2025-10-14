using Application.Common;
using Application.DTOs;
using Application.IServices;
using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDateTimeProvider _clock;

        public TicketService(ApplicationDbContext db, IDateTimeProvider clock)
        {
            _db = db; _clock = clock;
        }

        public async Task<long> CreateAsync(TicketCreateRequest req, CancellationToken ct = default)
        {
            var t = new Ticket
            {
                TenantId = req.TenantId,
                SubjectLine = req.SubjectLine,
                PriorityLevel = req.PriorityLevel,
                TicketStatusid = 1, // Open
                BodyText = req.BodyText,
                IsDeleted = false
            };
            _db.Add(t);
            await _db.SaveChangesAsync(ct);
            return t.TicketId;
        }

        public async Task AddReplyAsync(TicketReplyRequest req, CancellationToken ct = default)
        {
            var t = await _db.Set<Ticket>().FirstOrDefaultAsync(x => x.TicketId == req.TicketId && !x.IsDeleted, ct)
                    ?? throw new InvalidOperationException("Ticket not found.");

            // ملاحظة: جدول TicketChat في مخططك لا يحتوي TicketId لربط الرسائل بالتذكرة.
            // حالياً نُسجّل الرسالة كسجل محادثة عام حسب Tenant.
            // يُفضّل مستقبلاً إضافة عمود TicketId في TicketChat.
            var chat = new TicketChat
            {
                TenantId = req.TenantId,
                TenantText = req.ByAdmin ? string.Empty : req.Message,
                AdminText = req.ByAdmin ? req.Message : string.Empty,
                TenantTextAtUtc = _clock.UtcNow,
                AdminTextAtUtc = _clock.UtcNow,
                ChatLevel = 0
            };
            _db.Add(chat);

            if (req.ByAdmin)
                t.TicketStatusid = 2; // InProgress

            await _db.SaveChangesAsync(ct);
        }
    }
}
