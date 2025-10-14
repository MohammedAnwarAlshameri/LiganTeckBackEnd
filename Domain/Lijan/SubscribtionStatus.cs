using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class SubscribtionStatus
{
    public long SubStatusid { get; set; }

    public string SubStatusName { get; set; } = null!;

    public string? SubStatusdescription { get; set; }
}
