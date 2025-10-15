using Application.DTOs;
using Application.IServices;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TenantService: ITenantService
    {
        private readonly ApplicationDbContext _db;
        public TenantService(ApplicationDbContext db) => _db = db;

        public async Task<PagedResult<TenantListItemDto>> SearchAsync(string? q, long? statusId, int page, int pageSize, CancellationToken ct)
        {
            // base query: join على TenantStatus عشان نجيب الاسم
            var query =
                from t in _db.Tenants.AsNoTracking()
                join s in _db.TenantStatuses.AsNoTracking() on t.TenantStatusid equals s.TenantStatusid into gs
                from s in gs.DefaultIfEmpty()
                where !t.IsDeleted
                select new TenantListItemDto
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    Username = t.Username,
                    TenantEmail = t.TenantEmail,
                    PhoneNumber = t.PhoneNumber,
                    CreatedOn = t.CreatedOn,
                    TenantStatusName = s != null ? s.TenantStatusName : ""
                };

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.TenantName.Contains(term) ||
                    x.Username.Contains(term) ||
                    x.TenantEmail.Contains(term) ||
                    x.PhoneNumber.Contains(term));
            }

            if (statusId.HasValue)
            {
                // فلترة حسب الـ id (نحتاج اسم الحالة من جدول الحالة)
                query =
                    from row in query
                    join s in _db.TenantStatuses.AsNoTracking() on row.TenantStatusName equals s.TenantStatusName
                    where s.TenantStatusid == statusId.Value
                    select row;
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.TenantId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<TenantListItemDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<TenantListItemDto>> RecentAsync(int take, CancellationToken ct)
        {
            return await (
                from t in _db.Tenants.AsNoTracking()
                join s in _db.TenantStatuses.AsNoTracking() on t.TenantStatusid equals s.TenantStatusid into gs
                from s in gs.DefaultIfEmpty()
                where !t.IsDeleted
                orderby t.CreatedOn descending, t.TenantId descending
                select new TenantListItemDto
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    Username = t.Username,
                    TenantEmail = t.TenantEmail,
                    PhoneNumber = t.PhoneNumber,
                    CreatedOn = t.CreatedOn,
                    TenantStatusName = s != null ? s.TenantStatusName : ""
                }
            ).Take(take).ToListAsync(ct);
        }

        public async Task<TenantListItemDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await (
                from t in _db.Tenants.AsNoTracking()
                join s in _db.TenantStatuses.AsNoTracking() on t.TenantStatusid equals s.TenantStatusid into gs
                from s in gs.DefaultIfEmpty()
                where !t.IsDeleted && t.TenantId == id
                select new TenantListItemDto
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    Username = t.Username,
                    TenantEmail = t.TenantEmail,
                    PhoneNumber = t.PhoneNumber,
                    CreatedOn = t.CreatedOn,
                    TenantStatusName = s != null ? s.TenantStatusName : ""
                }
            ).SingleOrDefaultAsync(ct);
        }

        public async Task<bool> SoftDeleteAsync(long id, long? byUserId, CancellationToken ct)
        {
            var entity = await _db.Tenants.FirstOrDefaultAsync(x => x.TenantId == id && !x.IsDeleted, ct);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.ModifiedBy = byUserId;
            entity.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
