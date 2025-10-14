using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class Invoice
{
    public long InvoiceId { get; set; }

    public long SubscriptionId { get; set; }

    public long? PaymentMethodId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public decimal AmountSubtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? AmountTotal { get; set; }

    public int InvoiceStatusid { get; set; }

    public DateTime IssueDateUtc { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
