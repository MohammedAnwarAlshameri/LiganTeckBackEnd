using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> GetAsync(long invoiceId, CancellationToken ct = default);
        Task<List<InvoiceDto>> GetByTenantAsync(long tenantId, CancellationToken ct = default);
        Task<List<InvoiceDto>> GetBySubscriptionAsync(long subscriptionId, CancellationToken ct = default);

        Task<long> CreateAsync(CreateInvoiceRequest req, CancellationToken ct = default);
        Task<bool> MarkPaidAsync(long invoiceId, long paymentMethodId, DateTime? paidAtUtc, CancellationToken ct = default);
        Task<bool> CancelAsync(long invoiceId, CancellationToken ct = default);

        // ============ إدارة (Admin) ============
        Task<PagedResult<InvoiceDto>> SearchAsync(
            long? tenantId, long? subscriptionId, int? statusId,
            DateTime? fromUtc, DateTime? toUtc,
            int page, int pageSize, CancellationToken ct = default);
    }
}
