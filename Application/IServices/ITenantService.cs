using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ITenantService
    {
        Task<PagedResult<TenantListItemDto>> SearchAsync(string? q, long? statusId, int page, int pageSize, CancellationToken ct);
        Task<List<TenantListItemDto>> RecentAsync(int take, CancellationToken ct);
        Task<bool> SoftDeleteAsync(long id, long? byUserId, CancellationToken ct);
        Task<TenantListItemDto?> GetByIdAsync(long id, CancellationToken ct);
    }
}
