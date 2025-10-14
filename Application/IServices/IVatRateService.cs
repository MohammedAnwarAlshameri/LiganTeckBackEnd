using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IVatRateService
    {
        Task<List<VatRateVm>> ListAsync(CancellationToken ct = default);
        Task<long> CreateAsync(VatRateDto dto, CancellationToken ct = default);
        Task UpdateAsync(long id, VatRateDto dto, CancellationToken ct = default);
        Task DeleteAsync(long id, CancellationToken ct = default);
    }
}
