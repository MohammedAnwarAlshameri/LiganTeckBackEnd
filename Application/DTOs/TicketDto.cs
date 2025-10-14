using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TicketDto
    {
    }
    public class TicketCreateRequest
    {
        public long TenantId { get; set; }
        public string SubjectLine { get; set; } = null!;
        public string PriorityLevel { get; set; } = null!;
        public string BodyText { get; set; } = null!;

    }
   public class TicketReplyRequest
    {
        public long TicketId { get; set; }

        public long TenantId { get; set; }

        public bool ByAdmin { get; set; }

        public string Message { get; set; }

    }
    
}
