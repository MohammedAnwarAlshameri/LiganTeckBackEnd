using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class TenantStatus
{
    public long TenantStatusid { get; set; }

    public string TenantStatusName { get; set; } = null!;

    public string? TenantStatusdescription { get; set; }
}
