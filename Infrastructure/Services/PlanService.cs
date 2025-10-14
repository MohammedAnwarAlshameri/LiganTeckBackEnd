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
    public class PlanService:IPlanService
    {
        private readonly ApplicationDbContext _db;
        public PlanService(ApplicationDbContext db) => _db = db;

        public async Task<List<PlanVm>> ListAsync(bool includeDeleted, CancellationToken ct = default)
        {
            var q = _db.Set<Plan>().AsQueryable();
            if (!includeDeleted) q = q.Where(p => !p.IsDeleted);

            return await q.OrderByDescending(p => p.PlanId)
                .Select(p => new PlanVm
                {
                    PlanId = p.PlanId,
                    PlanCode = p.PlanCode,
                    PlanNameAr = p.PlanNameAr,
                    PlanNameEn = p.PlanNameEn,
                    PlanDetails = p.PlanDetails,
                    MonthlyPrice = p.MonthlyPrice,
                    IsActive = p.IsActive,
                    IsDeleted = p.IsDeleted,
                    CreatedOn = p.CreatedOn,
                    ModifiedOn = p.ModifiedOn
                }).ToListAsync(ct);
        }

        public async Task<PlanVm?> GetAsync(long id, CancellationToken ct = default)
        {
            return await _db.Set<Plan>()
                .Where(p => p.PlanId == id)
                .Select(p => new PlanVm
                {
                    PlanId = p.PlanId,
                    PlanCode = p.PlanCode,
                    PlanNameAr = p.PlanNameAr,
                    PlanNameEn = p.PlanNameEn,
                    PlanDetails = p.PlanDetails,
                    MonthlyPrice = p.MonthlyPrice,
                    IsActive = p.IsActive,
                    IsDeleted = p.IsDeleted,
                    CreatedOn = p.CreatedOn,
                    ModifiedOn = p.ModifiedOn
                }).SingleOrDefaultAsync(ct);
        }

        public async Task<long> CreateAsync(PlanUpsertDto dto, CancellationToken ct = default)
        {
            var code = string.IsNullOrWhiteSpace(dto.PlanCode)
                ? $"PLN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
                : dto.PlanCode!;

            var dup = await _db.Set<Plan>().AnyAsync(p => p.PlanCode == code && !p.IsDeleted, ct);
            if (dup) throw new InvalidOperationException("PlanCode already exists.");

            var e = new Plan
            {
                PlanCode = code,
                PlanNameAr = dto.PlanNameAr,
                PlanNameEn = dto.PlanNameEn,
                PlanDetails = dto.PlanDetails,
                MonthlyPrice = dto.MonthlyPrice,
                IsActive = dto.IsActive,
                IsDeleted = false
            };
            _db.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.PlanId;
        }

        public async Task UpdateAsync(long id, PlanUpsertDto dto, CancellationToken ct = default)
        {
            var e = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.PlanId == id && !p.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Plan not found.");

            if (!string.IsNullOrWhiteSpace(dto.PlanCode) && dto.PlanCode != e.PlanCode)
            {
                var dup = await _db.Set<Plan>()
                    .AnyAsync(p => p.PlanCode == dto.PlanCode && p.PlanId != id && !p.IsDeleted, ct);
                if (dup) throw new InvalidOperationException("PlanCode already exists.");
                e.PlanCode = dto.PlanCode!;
            }

            e.PlanNameAr = dto.PlanNameAr;
            e.PlanNameEn = dto.PlanNameEn;
            e.PlanDetails = dto.PlanDetails;
            e.MonthlyPrice = dto.MonthlyPrice;
            e.IsActive = dto.IsActive;

            await _db.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(long id, bool active, CancellationToken ct = default)
        {
            var e = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.PlanId == id && !p.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Plan not found.");
            e.IsActive = active;
            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(long id, CancellationToken ct = default)
        {
            var e = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.PlanId == id && !p.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Plan not found.");
            e.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }
}
