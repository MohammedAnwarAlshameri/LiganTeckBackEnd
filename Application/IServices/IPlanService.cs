using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IPlanService
    {
        Task<List<PlanVm>> ListAsync(bool includeDeleted, CancellationToken ct = default);
        Task<PlanVm?> GetAsync(long id, CancellationToken ct = default);
        Task<long> CreateAsync(PlanUpsertDto dto, CancellationToken ct = default);
        Task UpdateAsync(long id, PlanUpsertDto dto, CancellationToken ct = default);
        Task ActivateAsync(long id, bool active, CancellationToken ct = default);
        Task SoftDeleteAsync(long id, CancellationToken ct = default);
    }
}
