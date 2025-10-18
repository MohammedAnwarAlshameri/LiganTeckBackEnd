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

        private static readonly HashSet<string> AllowedPriorities =
            new(StringComparer.OrdinalIgnoreCase) { "Low", "Medium", "High", "Urgent" };

        private static string NormalizePriority(string p)
        {
            if (string.IsNullOrWhiteSpace(p)) return "Low";
            // نطبعها بشكل موحّد
            var val = p.Trim();
            if (AllowedPriorities.Contains(val))
                return char.ToUpper(val[0]) + val.Substring(1).ToLower();
            // جرّب lower
            var lower = val.ToLowerInvariant();
            return lower switch
            {
                "low" => "Low",
                "medium" => "Medium",
                "high" => "High",
                "urgent" => "Urgent",
                _ => throw new InvalidOperationException("Invalid priority. Allowed: Low, Medium, High, Urgent")
            };
        }

        public async Task<long> CreateAsync(TicketCreateRequest req, CancellationToken ct = default)
        {
            var t = new Ticket
            {
                TenantId = req.TenantId,
                SubjectLine = req.SubjectLine,
                PriorityLevel = NormalizePriority(req.PriorityLevel),
                TicketStatusid = 1, // Open
                BodyText = req.BodyText,
                AttachmentPath = string.IsNullOrWhiteSpace(req.AttachmentPath) ? null : req.AttachmentPath,
                CreatedOn = _clock.UtcNow,
                IsDeleted = false
            };

            _db.Add(t);
            await _db.SaveChangesAsync(ct);
            return t.TicketId;
        }

        public async Task AddReplyAsync(TicketReplyRequest req, CancellationToken ct = default)
        {
            var t = await _db.Set<Ticket>()
                             .FirstOrDefaultAsync(x => x.TicketId == req.TicketId && !x.IsDeleted, ct)
                     ?? throw new InvalidOperationException("Ticket not found.");

            var now = _clock.UtcNow;

            var chat = new TicketChat
            {
                TicketId = t.TicketId,
                TenantId = req.TenantId,
                TenantText = req.ByAdmin ? "" : req.Message,
                AdminText  = req.ByAdmin ? req.Message : "",
                TenantTextAtUtc = now,
                AdminTextAtUtc  = now,
                ChatLevel = req.ByAdmin ? 2 : 1
            };
            _db.Add(chat);

            if (req.ByAdmin)
                t.TicketStatusid = 2; // InProgress

            t.ModifiedOn = now;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<PagedResult<TicketDto>> GetAllTicketsAsync(
            string? q, string? tenantId, int? statusId, string? priority,
            int page, int pageSize, CancellationToken ct = default)
        {
            var query =
                from t in _db.Set<Ticket>().AsNoTracking()
                join ts in _db.Set<TicketStatus>().AsNoTracking()
                    on (long)t.TicketStatusid equals ts.TicketStatusid into gts
                from ts in gts.DefaultIfEmpty()
                join ten in _db.Set<Tenant>().AsNoTracking()
                    on t.TenantId equals ten.TenantId into gten
                from ten in gten.DefaultIfEmpty()
                where !t.IsDeleted
                select new TicketDto
                {
                    TicketId = t.TicketId,
                    TenantId = t.TenantId,
                    TenantName = ten != null ? ten.TenantName : "",
                    TenantEmail = ten != null ? ten.TenantEmail : "",
                    SubjectLine = t.SubjectLine,
                    PriorityLevel = t.PriorityLevel,
                    TicketStatusId = t.TicketStatusid,
                    TicketStatusName = ts != null ? ts.TicketStatusName : "",
                    CreatedOn = t.CreatedOn,
                    ModifiedOn = t.ModifiedOn,
                    ChatsCount = _db.Set<TicketChat>().Count(c => c.TicketId == t.TicketId)
                };

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.SubjectLine.Contains(term) ||
                    x.TenantName.Contains(term) ||
                    x.TenantEmail.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                if (long.TryParse(tenantId, out var tId))
                    query = query.Where(x => x.TenantId == tId);
                else
                    query = query.Where(x => x.TenantEmail.Contains(tenantId) || x.TenantName.Contains(tenantId));
            }

            if (statusId.HasValue)
                query = query.Where(x => x.TicketStatusId == statusId.Value);

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(x => x.PriorityLevel == NormalizePriority(priority));

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.TicketId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<TicketDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<TicketDto>> GetUserTicketsAsync(long tenantId, CancellationToken ct = default)
        {
            return await (
                from t in _db.Set<Ticket>().AsNoTracking()
                join ts in _db.Set<TicketStatus>().AsNoTracking()
                    on (long)t.TicketStatusid equals ts.TicketStatusid into gts
                from ts in gts.DefaultIfEmpty()
                join ten in _db.Set<Tenant>().AsNoTracking()
                    on t.TenantId equals ten.TenantId into gten
                from ten in gten.DefaultIfEmpty()
                where !t.IsDeleted && t.TenantId == tenantId
                orderby t.CreatedOn descending, t.TicketId descending
                select new TicketDto
                {
                    TicketId = t.TicketId,
                    TenantId = t.TenantId,
                    TenantName = ten != null ? ten.TenantName : "",
                    TenantEmail = ten != null ? ten.TenantEmail : "",
                    SubjectLine = t.SubjectLine,
                    PriorityLevel = t.PriorityLevel,
                    TicketStatusId = t.TicketStatusid,
                    TicketStatusName = ts != null ? ts.TicketStatusName : "",
                    CreatedOn = t.CreatedOn,
                    ModifiedOn = t.ModifiedOn,
                    ChatsCount = _db.Set<TicketChat>().Count(c => c.TicketId == t.TicketId)
                }
            ).ToListAsync(ct);
        }

        public async Task<bool> UpdateStatusAsync(long ticketId, int statusId, CancellationToken ct = default)
        {
            var entity = await _db.Set<Ticket>().FirstOrDefaultAsync(x => x.TicketId == ticketId && !x.IsDeleted, ct);
            if (entity == null) return false;

            entity.TicketStatusid = statusId;
            entity.ModifiedOn = _clock.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdatePriorityAsync(long ticketId, string priority, CancellationToken ct = default)
        {
            var entity = await _db.Set<Ticket>().FirstOrDefaultAsync(x => x.TicketId == ticketId && !x.IsDeleted, ct);
            if (entity == null) return false;

            entity.PriorityLevel = NormalizePriority(priority);
            entity.ModifiedOn = _clock.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> BulkUpdateStatusAsync(List<long> ticketIds, int statusId, CancellationToken ct = default)
        {
            if (ticketIds == null || ticketIds.Count == 0) return true;

            var items = await _db.Set<Ticket>()
                                 .Where(t => ticketIds.Contains(t.TicketId) && !t.IsDeleted)
                                 .ToListAsync(ct);

            if (items.Count == 0) return false;

            var now = _clock.UtcNow;
            foreach (var t in items)
            {
                t.TicketStatusid = statusId;
                t.ModifiedOn = now;
            }
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AddChatAsync(long ticketId, long tenantId, string message, bool isAdmin, CancellationToken ct = default)
        {
            var ticket = await _db.Set<Ticket>().FirstOrDefaultAsync(x => x.TicketId == ticketId && !x.IsDeleted, ct);
            if (ticket == null) return false;

            if (ticket.TenantId != tenantId && !isAdmin)
                throw new InvalidOperationException("Tenant mismatch for ticket.");

            var now = _clock.UtcNow;

            var chat = new TicketChat
            {
                TicketId = ticket.TicketId,
                TenantId = ticket.TenantId,
                TenantText = isAdmin ? "" : message,
                AdminText  = isAdmin ? message : "",
                TenantTextAtUtc = now,
                AdminTextAtUtc  = now,
                ChatLevel = isAdmin ? 2 : 1
            };

            _db.Add(chat);
            ticket.ModifiedOn = now;

            // لو تحب: عند أول رد من الإدارة غيّر الحالة
            if (isAdmin && ticket.TicketStatusid == 1)
                ticket.TicketStatusid = 2;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
