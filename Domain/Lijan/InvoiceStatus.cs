using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class InvoiceStatus
{
    public long InvoiceStatusid { get; set; }

    public string InvoiceStatusName { get; set; } = null!;

    public string? InvoiceStatusdescription { get; set; }
}
