using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class Tenant
{
    public long TenantId { get; set; }

    public string TenantName { get; set; } = null!;

    public Guid TenantKey { get; set; }
     
    public string Username {  get; set; } = null!;

    public string TenantEmail { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string TenantPassword { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public long TenantStatusid { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
