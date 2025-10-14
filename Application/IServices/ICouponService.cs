using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ICouponService
    {
        Task<List<CouponVm>> ListAsync(bool includeDeleted, bool? active, CancellationToken ct = default);
        Task<CouponVm?> GetAsync(long id, CancellationToken ct = default);
        Task<long> CreateAsync(CouponUpsertDto dto, CancellationToken ct = default);
        Task UpdateAsync(long id, CouponUpsertDto dto, CancellationToken ct = default);
        Task ActivateAsync(long id, bool active, CancellationToken ct = default);
        Task SoftDeleteAsync(long id, CancellationToken ct = default);
    }
}
