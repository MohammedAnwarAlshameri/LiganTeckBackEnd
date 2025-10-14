using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class Plan
{
    public long PlanId { get; set; }

    public string PlanCode { get; set; } = null!;

    public string PlanNameAr { get; set; } = null!;

    public string PlanNameEn { get; set; } = null!;

    public string PlanDetails { get; set; } = null!;

    public decimal MonthlyPrice { get; set; }

    public bool IsActive { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
