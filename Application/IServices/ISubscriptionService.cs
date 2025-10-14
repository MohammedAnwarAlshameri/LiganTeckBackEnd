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
    }
}
