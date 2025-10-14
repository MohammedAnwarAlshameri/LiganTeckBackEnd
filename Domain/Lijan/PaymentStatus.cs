using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class PaymentStatus
{
    public long PaymentStatusid { get; set; }

    public string PaymentStatusName { get; set; } = null!;

    public string? PaymentStatusdescription { get; set; }
}
