using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class Ticket
{
    public long TicketId { get; set; }

    public long TenantId { get; set; }


    public string? AttachmentPath { get; set; } // ← اختياري
    public string SubjectLine { get; set; } = null!;

    public string PriorityLevel { get; set; } = null!;

    public int TicketStatusid { get; set; }

    public string? BodyText { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
