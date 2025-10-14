using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PaymentMethodDto
    {
    }
    public sealed class PaymentMethodVm
    {
        public long PaymentMethodId { get; set; }
        public long TenantId { get; set; }
        public string HolderName { get; set; } = null!;
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }
        public byte? ExpMonth { get; set; }
        public short? ExpYear { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
    public sealed class PaymentMethodUpsertDto
    {
        public long TenantId { get; set; }
        public string HolderName { get; set; } = null!;
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }
        public byte? ExpMonth { get; set; }
        public short? ExpYear { get; set; }
        public string? TokenRef { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
