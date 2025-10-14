using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CouponDto
    {
    }
    public sealed class CouponVm
    {
        public long CouponId { get; set; }
        public string CouponCode { get; set; } = null!;
        public decimal DiscountPercent { get; set; }
        public int? MaxRedemptions { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ValidFromUtc { get; set; }
        public DateTime? ValidToUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
    public sealed class CouponUpsertDto
    {
        public string CouponCode { get; set; } = null!;
        public decimal DiscountPercent { get; set; }       // 0..100
        public int? MaxRedemptions { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFromUtc { get; set; }
        public DateTime? ValidToUtc { get; set; }
    }

}
