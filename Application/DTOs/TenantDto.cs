using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public sealed class TenantListItemDto
    {
        public long TenantId { get; set; }
        public string TenantName { get; set; } = "";
        public string Username { get; set; } = "";
        public string TenantEmail { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public DateTime? CreatedOn { get; set; }
        public string TenantStatusName { get; set; } = "";
    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}
