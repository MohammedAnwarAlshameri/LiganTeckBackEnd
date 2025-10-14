using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class PaymentMethod
{
    public long PaymentMethodId { get; set; }

    public long TenantId { get; set; }

    public string HolderName { get; set; } = null!;

    public string? CardBrand { get; set; }

    public string? CardLast4 { get; set; }

    public byte? ExpMonth { get; set; }

    public short? ExpYear { get; set; }

    public string? TokenRef { get; set; }

    public bool IsDefault { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
