using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PaymentDto
    {
        public long PaymentId { get; set; }

        public long InvoiceId { get; set; }

        public string? ProviderRef { get; set; }

        public decimal AmountPaid { get; set; }

        public string CurrencyCode { get; set; } = null!;

        public int PaymentStatusid { get; set; }

        public DateTime? PaidAtUtc { get; set; }

        public string? FailureCode { get; set; }

        public string? FailureMessage { get; set; }

     

        public bool IsDeleted { get; set; }
    }
    public class ConfirmPaymentRequest
    {
        public long InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public bool Success { get; set; }
        public string? ProviderRef { get; set; }
        public string? FailureCode { get; set; }
        public string? FailureMessage { get; set; }

    }
   
}
