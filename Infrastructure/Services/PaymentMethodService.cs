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
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly ApplicationDbContext _db;
        public PaymentMethodService(ApplicationDbContext db) => _db = db;

        public async Task<List<PaymentMethodVm>> ListByTenantAsync(long tenantId, CancellationToken ct = default)
        {
            return await _db.Set<PaymentMethod>()
                .Where(pm => pm.TenantId == tenantId && !pm.IsDeleted)
                .OrderByDescending(pm => pm.PaymentMethodId)
                .Select(pm => new PaymentMethodVm
                {
                    PaymentMethodId = pm.PaymentMethodId,
                    TenantId = pm.TenantId,
                    HolderName = pm.HolderName,
                    CardBrand = pm.CardBrand,
                    CardLast4 = pm.CardLast4,
                    ExpMonth = pm.ExpMonth,
                    ExpYear = pm.ExpYear,
                    IsDefault = pm.IsDefault,
                    CreatedOn = pm.CreatedOn
                }).ToListAsync(ct);
        }

        public async Task<long> CreateAsync(PaymentMethodUpsertDto dto, CancellationToken ct = default)
        {
            var okTenant = await _db.Set<Tenant>().AnyAsync(t => t.TenantId == dto.TenantId && !t.IsDeleted, ct);
            if (!okTenant) throw new InvalidOperationException("Invalid tenant.");

            if (dto.IsDefault)
            {
                var others = await _db.Set<PaymentMethod>()
                    .Where(pm => pm.TenantId == dto.TenantId && !pm.IsDeleted && pm.IsDefault)
                    .ToListAsync(ct);
                foreach (var o in others) o.IsDefault = false;
            }

            var e = new PaymentMethod
            {
                TenantId = dto.TenantId,
                HolderName = dto.HolderName,
                CardBrand = dto.CardBrand,
                CardLast4 = dto.CardLast4,
                ExpMonth = dto.ExpMonth,
                ExpYear = dto.ExpYear,
                TokenRef = dto.TokenRef,
                IsDefault = dto.IsDefault,
                IsDeleted = false
            };
            _db.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.PaymentMethodId;
        }

        public async Task UpdateAsync(long id, PaymentMethodUpsertDto dto, CancellationToken ct = default)
        {
            var e = await _db.Set<PaymentMethod>().FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && !pm.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Payment method not found.");

            if (e.TenantId != dto.TenantId)
                throw new InvalidOperationException("TenantId cannot be changed.");

            e.HolderName = dto.HolderName;
            e.CardBrand = dto.CardBrand;
            e.CardLast4 = dto.CardLast4;
            e.ExpMonth = dto.ExpMonth;
            e.ExpYear = dto.ExpYear;
            e.TokenRef = dto.TokenRef;

            if (dto.IsDefault && !e.IsDefault)
            {
                var others = await _db.Set<PaymentMethod>()
                    .Where(pm => pm.TenantId == e.TenantId && !pm.IsDeleted && pm.IsDefault)
                    .ToListAsync(ct);
                foreach (var o in others) o.IsDefault = false;
                e.IsDefault = true;
            }
            else if (!dto.IsDefault && e.IsDefault)
            {
                e.IsDefault = false;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task MakeDefaultAsync(long id, CancellationToken ct = default)
        {
            var e = await _db.Set<PaymentMethod>().FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && !pm.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Payment method not found.");

            var others = await _db.Set<PaymentMethod>()
                .Where(pm => pm.TenantId == e.TenantId && !pm.IsDeleted && pm.IsDefault && pm.PaymentMethodId != id)
                .ToListAsync(ct);

            foreach (var o in others) o.IsDefault = false;
            e.IsDefault = true;

            await _db.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(long id, CancellationToken ct = default)
        {
            var e = await _db.Set<PaymentMethod>().FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && !pm.IsDeleted, ct)
                    ?? throw new KeyNotFoundException("Payment method not found.");

            e.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }
}
