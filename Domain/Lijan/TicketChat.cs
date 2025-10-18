using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class TicketChat
{
    public long ChatId { get; set; }

    public long TicketId { get; set; }
    public long TenantId { get; set; }

    public string TenantText { get; set; } = null!;

    public string AdminText { get; set; } = null!;

    public DateTime TenantTextAtUtc { get; set; }

    public DateTime AdminTextAtUtc { get; set; }

    public int ChatLevel { get; set; }
}
