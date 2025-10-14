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
    public class CouponService:ICouponService
    {
        private readonly ApplicationDbContext _db;
        public CouponService(ApplicationDbContext db) => _db = db;

        public async Task<List<CouponVm>> ListAsync(bool includeDeleted, bool? active, CancellationToken ct = default)
        {
            var q = _db.Set<Coupon>().AsQueryable();
            if (!includeDeleted) q = q.Where(c => !c.IsDeleted);
            if (active.HasValue) q = q.Where(c => c.IsActive == active.Value);

            return await q.OrderByDescending(c => c.CouponId)
                .Select(c => new CouponVm
                {
                    CouponId = c.CouponId,
                    CouponCode = c.CouponCode,
                    DiscountPercent = c.DiscountPercent,
                    MaxRedemptions = c.MaxRedemptions,
                    IsActive = c.IsActive,
                    ValidFromUtc = c.ValidFromUtc,
                    ValidToUtc = c.ValidToUtc,
                    IsDeleted = c.IsDeleted,
                    CreatedOn = c.CreatedOn
                }).ToListAsync(ct);
        }

        public async Task<CouponVm?> GetAsync(long id, CancellationToken ct = default)
        {
            return await _db.Set<Coupon>()
                .Where(c => c.CouponId == id)
                .Select(c => new CouponVm
                {
                    CouponId = c.CouponId,
                    CouponCode = c.CouponCode,
                    DiscountPercent = c.DiscountPercent,
                    MaxRedemptions = c.MaxRedemptions,
                    IsActive = c.IsActive,
                    ValidFromUtc = c.ValidFromUtc,
                    ValidToUtc = c.ValidToUtc,
                    IsDeleted = c.IsDeleted,
                    CreatedOn = c.CreatedOn
                }).SingleOrDefaultAsync(ct);
        }

        public async Task<long> CreateAsync(CouponUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
                throw new InvalidOperationException("DiscountPercent must be between 0 and 100.");

            var dup = await _db.Set<Coupon>().AnyAsync(c => c.CouponCode == dto.CouponCode && !c.IsDeleted, ct);
            if (dup) throw new InvalidOperationException("CouponCode already exists.");

            var e = new Coupon
            {
                CouponCode = dto.CouponCode,
                DiscountPercent = dto.DiscountPercent,
                MaxRedemptions = dto.MaxRedemptions,
                IsActive = dto.IsActive,
                ValidFromUtc = dto.ValidFromUtc,
                ValidToUtc = dto.ValidToUtc,
                IsDeleted = false
            };
            _db.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.CouponId;
        }

        public async Task UpdateAsync(long id, CouponUpsertDto dto, CancellationToken ct = default)
        {
            var e = await _db.Set<Coupon>().FirstOrDefaultAsync(c => c.CouponId == id && !c.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Coupon not found.");

            if (e.CouponCode != dto.CouponCode)
            {
                var dup = await _db.Set<Coupon>()
                    .AnyAsync(c => c.CouponCode == dto.CouponCode && c.CouponId != id && !c.IsDeleted, ct);
                if (dup) throw new InvalidOperationException("CouponCode already exists.");
                e.CouponCode = dto.CouponCode;
            }

            if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
                throw new InvalidOperationException("DiscountPercent must be between 0 and 100.");

            e.DiscountPercent = dto.DiscountPercent;
            e.MaxRedemptions = dto.MaxRedemptions;
            e.IsActive = dto.IsActive;
            e.ValidFromUtc = dto.ValidFromUtc;
            e.ValidToUtc = dto.ValidToUtc;

            await _db.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(long id, bool active, CancellationToken ct = default)
        {
            var e = await _db.Set<Coupon>().FirstOrDefaultAsync(c => c.CouponId == id && !c.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Coupon not found.");
            e.IsActive = active;
            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(long id, CancellationToken ct = default)
        {
            var e = await _db.Set<Coupon>().FirstOrDefaultAsync(c => c.CouponId == id && !c.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Coupon not found.");
            e.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }
}
