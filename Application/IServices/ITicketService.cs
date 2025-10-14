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
    }
}
