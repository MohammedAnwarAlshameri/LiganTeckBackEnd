using Application.Common;
using Application.DTOs;
using Application.IServices;
using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDateTimeProvider _clock;

        public PaymentService(ApplicationDbContext db, IDateTimeProvider clock)
        {
            _db = db; _clock = clock;
        }

        public async Task<bool> ConfirmAsync(ConfirmPaymentRequest req, CancellationToken ct = default)
        {
            var inv = await _db.Set<Invoice>().FirstOrDefaultAsync(i => i.InvoiceId == req.InvoiceId && !i.IsDeleted, ct);
            if (inv == null) return false;

            var pay = new Payment
            {
                InvoiceId = inv.InvoiceId,
                ProviderRef = req.ProviderRef,
                AmountPaid = req.AmountPaid,
                CurrencyCode = req.CurrencyCode,
                PaymentStatusid = req.Success ? 2 : 3, // 1=Pending,2=Succeeded,3=Failed
                PaidAtUtc = req.Success ? _clock.UtcNow : null,
                FailureCode = req.Success ? null : req.FailureCode,
                FailureMessage = req.Success ? null : req.FailureMessage,
                IsDeleted = false
            };
            _db.Add(pay);

            if (req.Success)
            {
                inv.InvoiceStatusid = 2; // Paid
                inv.PaidAtUtc = _clock.UtcNow;

                var sub = await _db.Set<Subscription>().FirstOrDefaultAsync(s => s.SubscriptionId == inv.SubscriptionId && !s.IsDeleted, ct);
                if (sub != null)
                {
                    sub.SubStatusid = 1; // Active
                    if (sub.NextBillingUtc == null && sub.AutoRenew)
                        sub.NextBillingUtc = sub.StartDateUtc.AddMonths(sub.MonthsCount);
                }
            }
            else
            {
                inv.InvoiceStatusid = 3; // Failed
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
