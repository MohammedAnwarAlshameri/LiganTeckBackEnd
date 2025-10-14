using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public sealed class VatRateDto
    {
        public string CountryCode { get; set; } = "SA";   // 2 chars
        public decimal VatPercent { get; set; }           // 0..100
    }
    public sealed class VatRateVm
    {
        public long VatRateId { get; set; }
        public string CountryCode { get; set; } = null!;
        public decimal VatPercent { get; set; }
    }

}
