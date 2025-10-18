using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request, CancellationToken ct = default);
        Task<bool> RenewAsync(long subscriptionId, CancellationToken ct = default);

        // ============ إدارة (Admin) ============
        Task<PagedResult<SubscriptionListItemDto>> AdminSearchAsync(
            long? tenantId, long? planId, int? statusId,
            DateTime? fromUtc, DateTime? toUtc,
            int page, int pageSize, CancellationToken ct = default);
    }
}
