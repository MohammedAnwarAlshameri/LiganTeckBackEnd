using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SubscriptionDto
    {
        public long SubscriptionId { get; set; }
        public long TenantId { get; set; }
        public long PlanId { get; set; }
        public int MonthsCount { get; set; }
        public bool AutoRenew { get; set; }
        public int SubStatusid { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public long? CouponId { get; set; }

        // نتركها اختيارية
        public InvoiceDto? Invoice { get; set; }
    }
    public class CreateSubscriptionRequest 
    {
        public long TenantId { get; set; }
        public long PlanId { get; set; }
        public int MonthsCount { get; set; }
        public bool AutoRenew { get; set; }
        public string? CouponCode { get; set; }

    }
    
}
