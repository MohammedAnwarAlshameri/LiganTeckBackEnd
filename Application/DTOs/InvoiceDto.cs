using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class InvoiceDto
    {
        public long InvoiceId { get; set; }

        public string InvoiceNumber { get; set; } = null!;

        public string CurrencyCode { get; set; } = "SAR";

        public decimal AmountSubtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal? AmountTotal { get; set; }

        public int InvoiceStatusid { get; set; }

        public DateTime IssueDateUtc { get; set; }

       

       
    }
}
