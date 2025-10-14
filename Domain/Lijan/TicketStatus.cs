using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class TicketStatus
{
    public long TicketStatusid { get; set; }

    public string TicketStatusName { get; set; } = null!;

    public string? TicketStatusdescription { get; set; }
}
