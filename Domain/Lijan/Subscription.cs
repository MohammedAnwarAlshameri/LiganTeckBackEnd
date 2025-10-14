using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class Subscription
{
    public long SubscriptionId { get; set; }

    public long TenantId { get; set; }

    public long PlanId { get; set; }

    public int MonthsCount { get; set; }

    public bool AutoRenew { get; set; }

    public int SubStatusid { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime? EndDateUtc { get; set; }

    public DateTime? NextBillingUtc { get; set; }

    public long? CouponId { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
