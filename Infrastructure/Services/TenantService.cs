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
    public class TenantService : ITenantService
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
                select new { t, s };

            // تطبيق فلترة البحث
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.t.TenantName.Contains(term) ||
                    x.t.Username.Contains(term) ||
                    x.t.TenantEmail.Contains(term) ||
                    x.t.PhoneNumber.Contains(term));
            }

            // تطبيق فلترة الحالة
            if (statusId.HasValue)
            {
                query = query.Where(x => x.t.TenantStatusid == statusId.Value);
            }

            // حساب العدد الإجمالي
            var total = await query.CountAsync(ct);

            // جلب البيانات مع التقسيم
            var items = await query
                .OrderByDescending(x => x.t.CreatedOn)
                .ThenByDescending(x => x.t.TenantId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TenantListItemDto
                {
                    TenantId = x.t.TenantId,
                    TenantName = x.t.TenantName,
                    Username = x.t.Username,
                    TenantEmail = x.t.TenantEmail,
                    PhoneNumber = x.t.PhoneNumber,
                    CreatedOn = x.t.CreatedOn,
                    TenantStatusName = x.s != null ? x.s.TenantStatusName : ""
                })
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