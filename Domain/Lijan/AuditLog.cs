using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class AuditLog
{
    public long LogId { get; set; }

    public long? TenantId { get; set; }

    public string ActionName { get; set; } = null!;

    public string? EntityType { get; set; }

    public long? EntityId { get; set; }

    public string? MetaJson { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
