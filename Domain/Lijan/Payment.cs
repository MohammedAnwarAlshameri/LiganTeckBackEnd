using System;
using System.Collections.Generic;

namespace Domain.Lijan;

public partial class Payment
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

    public long? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }
}
