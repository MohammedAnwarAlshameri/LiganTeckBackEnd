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
    public class VatRateService : IVatRateService
    {
        private readonly ApplicationDbContext _db;
        public VatRateService(ApplicationDbContext db) => _db = db;

        public async Task<List<VatRateVm>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Set<Vatrate>()
                .OrderBy(v => v.CountryCode)
                .Select(v => new VatRateVm
                {
                    VatRateId = v.VatRateId,
                    CountryCode = v.CountryCode,
                    VatPercent = v.VatPercent
                }).ToListAsync(ct);
        }

        public async Task<long> CreateAsync(VatRateDto dto, CancellationToken ct = default)
        {
            var code = dto.CountryCode.Trim().ToUpperInvariant();
            if (dto.VatPercent < 0 || dto.VatPercent > 100)
                throw new InvalidOperationException("VatPercent must be between 0 and 100.");

            var dup = await _db.Set<Vatrate>().AnyAsync(v => v.CountryCode == code, ct);
            if (dup) throw new InvalidOperationException("VAT for this country already exists.");

            var e = new Vatrate { CountryCode = code, VatPercent = dto.VatPercent };
            _db.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.VatRateId;
        }

        public async Task UpdateAsync(long id, VatRateDto dto, CancellationToken ct = default)
        {
            var e = await _db.Set<Vatrate>().FirstOrDefaultAsync(v => v.VatRateId == id, ct)
                    ?? throw new KeyNotFoundException("VAT not found.");

            var code = dto.CountryCode.Trim().ToUpperInvariant();
            if (dto.VatPercent < 0 || dto.VatPercent > 100)
                throw new InvalidOperationException("VatPercent must be between 0 and 100.");

            if (code != e.CountryCode)
            {
                var dup = await _db.Set<Vatrate>().AnyAsync(v => v.CountryCode == code, ct);
                if (dup) throw new InvalidOperationException("VAT for this country already exists.");
                e.CountryCode = code;
            }

            e.VatPercent = dto.VatPercent;
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(long id, CancellationToken ct = default)
        {
            var e = await _db.Set<Vatrate>().FirstOrDefaultAsync(v => v.VatRateId == id, ct)
                    ?? throw new KeyNotFoundException("VAT not found.");

            _db.Remove(e);
            await _db.SaveChangesAsync(ct);
        }
    }
}
