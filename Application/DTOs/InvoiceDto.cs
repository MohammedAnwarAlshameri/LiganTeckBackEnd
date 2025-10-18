namespace Application.DTOs
{
    public sealed class InvoiceDto
    {
        public long InvoiceId { get; set; }
        public long SubscriptionId { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public string CurrencyCode { get; set; } = "SAR";
        public decimal AmountSubtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal AmountTotal { get; set; }
        public int InvoiceStatusid { get; set; }
        public DateTime IssueDateUtc { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    }

    public sealed class CreateInvoiceRequest      // عادة لن نحتاجها مع الاشتراك؛ لكنها مفيدة إن أردت إنشاء فاتورة مستقلة
    {
        public long SubscriptionId { get; set; }
        public string CurrencyCode { get; set; } = "SAR";
        public decimal AmountSubtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public int InvoiceStatusid { get; set; } = 1; // Pending
    }

    public sealed class PayInvoiceRequest
    {
        public long PaymentMethodId { get; set; }   // طريقة الدفع المختارة
        public DateTime? PaidAtUtc { get; set; }    // اختياري: لو أردت أن ترسل التاريخ بنفسك
    }
}
