using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PlanDto
    {
        public long PlanId { get; set; }

        public string PlanNameAr { get; set; } = null!;

        public string PlanNameEn { get; set; } = null!;

       // public string PlanDetails { get; set; } = null!;

        public decimal MonthlyPrice { get; set; }

        
    }
    public sealed class PlanVm
    {
        public long PlanId { get; set; }
        public string PlanCode { get; set; } = null!;
        public string PlanNameAr { get; set; } = null!;
        public string PlanNameEn { get; set; } = null!;
        public string PlanDetails { get; set; } = null!;
        public decimal MonthlyPrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
    public sealed class PlanUpsertDto
    {
        public string? PlanCode { get; set; }               // إن تُركت null نولّد
        public string PlanNameAr { get; set; } = null!;
        public string PlanNameEn { get; set; } = null!;
        public string PlanDetails { get; set; } = null!;
        public decimal MonthlyPrice { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
