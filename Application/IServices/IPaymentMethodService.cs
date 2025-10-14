using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IPaymentMethodService
    {
        Task<List<PaymentMethodVm>> ListByTenantAsync(long tenantId, CancellationToken ct = default);
        Task<long> CreateAsync(PaymentMethodUpsertDto dto, CancellationToken ct = default);
        Task UpdateAsync(long id, PaymentMethodUpsertDto dto, CancellationToken ct = default);
        Task MakeDefaultAsync(long id, CancellationToken ct = default);
        Task SoftDeleteAsync(long id, CancellationToken ct = default);
    }
}
