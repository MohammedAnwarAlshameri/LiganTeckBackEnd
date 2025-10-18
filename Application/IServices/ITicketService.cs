using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ITicketService
    {
        Task<long> CreateAsync(TicketCreateRequest req, CancellationToken ct = default);
        Task AddReplyAsync(TicketReplyRequest req, CancellationToken ct = default);

        Task<PagedResult<TicketDto>> GetAllTicketsAsync(
            string? q, string? tenantId, int? statusId, string? priority,
            int page, int pageSize, CancellationToken ct = default);

        Task<List<TicketDto>> GetUserTicketsAsync(long tenantId, CancellationToken ct = default);

        Task<bool> UpdateStatusAsync(long ticketId, int statusId, CancellationToken ct = default);
        Task<bool> UpdatePriorityAsync(long ticketId, string priority, CancellationToken ct = default);
        Task<bool> BulkUpdateStatusAsync(List<long> ticketIds, int statusId, CancellationToken ct = default);

        Task<bool> AddChatAsync(long ticketId, long tenantId, string message, bool isAdmin, CancellationToken ct = default);

    }
}
