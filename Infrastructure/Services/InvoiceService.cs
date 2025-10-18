using Application.DTOs;
using Application.IServices;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _db;
        public InvoiceService(ApplicationDbContext db) => _db = db;

        public async Task<InvoiceDto> GetAsync(long invoiceId, CancellationToken ct = default)
        {
            var inv = await _db.Set<Domain.Lijan.Invoice>()
                .AsNoTracking()
                .Where(i => i.InvoiceId == invoiceId && !i.IsDeleted)
                .Select(i => new InvoiceDto
                {
                    InvoiceId = i.InvoiceId,
                    SubscriptionId = i.SubscriptionId,
                    InvoiceNumber = i.InvoiceNumber,
                    CurrencyCode = i.CurrencyCode,
                    AmountSubtotal = i.AmountSubtotal,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount,
                    AmountTotal = (i.AmountSubtotal - i.DiscountAmount + i.TaxAmount),
                    InvoiceStatusid = i.InvoiceStatusid,
                    IssueDateUtc = i.IssueDateUtc,
                    DueDateUtc = i.DueDateUtc,
                    PaidAtUtc = i.PaidAtUtc
                })
                .SingleOrDefaultAsync(ct);

            if (inv == null)
                throw new InvalidOperationException("Invoice not found.");

            return inv;
        }

        public async Task<List<InvoiceDto>> GetByTenantAsync(long tenantId, CancellationToken ct = default)
        {
            // لا يوجد navigation؛ نعمل join حسب SubscriptionId->TenantId
            var query =
                from i in _db.Set<Domain.Lijan.Invoice>().AsNoTracking()
                join s in _db.Set<Domain.Lijan.Subscription>().AsNoTracking()
                    on i.SubscriptionId equals s.SubscriptionId
                where !i.IsDeleted && !s.IsDeleted && s.TenantId == tenantId
                orderby i.InvoiceId descending
                select new InvoiceDto
                {
                    InvoiceId = i.InvoiceId,
                    SubscriptionId = i.SubscriptionId,
                    InvoiceNumber = i.InvoiceNumber,
                    CurrencyCode = i.CurrencyCode,
                    AmountSubtotal = i.AmountSubtotal,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount,
                    AmountTotal = (i.AmountSubtotal - i.DiscountAmount + i.TaxAmount),
                    InvoiceStatusid = i.InvoiceStatusid,
                    IssueDateUtc = i.IssueDateUtc,
                    DueDateUtc = i.DueDateUtc,
                    PaidAtUtc = i.PaidAtUtc
                };

            return await query.ToListAsync(ct);
        }

        public async Task<List<InvoiceDto>> GetBySubscriptionAsync(long subscriptionId, CancellationToken ct = default)
        {
            var query =
                from i in _db.Set<Domain.Lijan.Invoice>().AsNoTracking()
                where !i.IsDeleted && i.SubscriptionId == subscriptionId
                orderby i.InvoiceId descending
                select new InvoiceDto
                {
                    InvoiceId = i.InvoiceId,
                    SubscriptionId = i.SubscriptionId,
                    InvoiceNumber = i.InvoiceNumber,
                    CurrencyCode = i.CurrencyCode,
                    AmountSubtotal = i.AmountSubtotal,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount,
                    AmountTotal = (i.AmountSubtotal - i.DiscountAmount + i.TaxAmount),
                    InvoiceStatusid = i.InvoiceStatusid,
                    IssueDateUtc = i.IssueDateUtc,
                    DueDateUtc = i.DueDateUtc,
                    PaidAtUtc = i.PaidAtUtc
                };

            return await query.ToListAsync(ct);
        }

        public async Task<long> CreateAsync(CreateInvoiceRequest req, CancellationToken ct = default)
        {
            var inv = new Domain.Lijan.Invoice
            {
                SubscriptionId = req.SubscriptionId,
                InvoiceNumber = $"INV-{req.SubscriptionId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                CurrencyCode = req.CurrencyCode ?? "SAR",
                AmountSubtotal = req.AmountSubtotal,
                DiscountAmount = req.DiscountAmount,
                TaxAmount = req.TaxAmount,
                // نخزّنها أيضًا داخل الجدول (بالرغم من أنها nullable)
                AmountTotal = req.AmountSubtotal - req.DiscountAmount + req.TaxAmount,
                InvoiceStatusid = req.InvoiceStatusid, // 1 = Pending
                IssueDateUtc = DateTime.UtcNow,
                DueDateUtc = DateTime.UtcNow.AddDays(7),
                IsDeleted = false
            };

            _db.Add(inv);
            await _db.SaveChangesAsync(ct);
            return inv.InvoiceId;
        }

        public async Task<bool> MarkPaidAsync(long invoiceId, long paymentMethodId, DateTime? paidAtUtc, CancellationToken ct = default)
        {
            var inv = await _db.Set<Domain.Lijan.Invoice>()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted, ct);
            if (inv == null) return false;

            inv.PaymentMethodId = paymentMethodId;
            inv.PaidAtUtc = paidAtUtc ?? DateTime.UtcNow;
            inv.InvoiceStatusid = 2; // Paid
            inv.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CancelAsync(long invoiceId, CancellationToken ct = default)
        {
            var inv = await _db.Set<Domain.Lijan.Invoice>()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted, ct);
            if (inv == null) return false;

            inv.InvoiceStatusid = 3; // Cancelled
            inv.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }
        // ============ إدارة (Admin) - البحث مع ترقيم الصفحات ============
        public async Task<PagedResult<InvoiceDto>> SearchAsync(
            long? tenantId, long? subscriptionId, int? statusId,
            DateTime? fromUtc, DateTime? toUtc,
            int page, int pageSize, CancellationToken ct = default)
        {
            // نعمل join اختياري مع الاشتراكات إذا أردنا الفلترة بـ tenantId
            var query =
                from i in _db.Set<Domain.Lijan.Invoice>().AsNoTracking()
                join s in _db.Set<Domain.Lijan.Subscription>().AsNoTracking()
                    on i.SubscriptionId equals s.SubscriptionId into gs
                from s in gs.DefaultIfEmpty()
                where !i.IsDeleted
                select new { i, s };

            if (tenantId.HasValue)
            {
                // نفلتر حسب TenantId من جدول الاشتراك
                query = query.Where(x => x.s != null && !x.s.IsDeleted && x.s.TenantId == tenantId.Value);
            }

            if (subscriptionId.HasValue)
            {
                query = query.Where(x => x.i.SubscriptionId == subscriptionId.Value);
            }

            if (statusId.HasValue)
            {
                query = query.Where(x => x.i.InvoiceStatusid == statusId.Value);
            }

            if (fromUtc.HasValue)
            {
                query = query.Where(x => x.i.IssueDateUtc >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(x => x.i.IssueDateUtc <= toUtc.Value);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.i.IssueDateUtc)
                .ThenByDescending(x => x.i.InvoiceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InvoiceDto
                {
                    InvoiceId = x.i.InvoiceId,
                    SubscriptionId = x.i.SubscriptionId,
                    InvoiceNumber = x.i.InvoiceNumber,
                    CurrencyCode = x.i.CurrencyCode,
                    AmountSubtotal = x.i.AmountSubtotal,
                    DiscountAmount = x.i.DiscountAmount,
                    TaxAmount = x.i.TaxAmount,
                    // إن كانت AmountTotal غير محسوبة في الجدول نعيد حسابها
                    AmountTotal = x.i.AmountTotal ?? (x.i.AmountSubtotal - x.i.DiscountAmount + x.i.TaxAmount),
                    InvoiceStatusid = x.i.InvoiceStatusid,
                    IssueDateUtc = x.i.IssueDateUtc,
                    DueDateUtc = x.i.DueDateUtc,
                    PaidAtUtc = x.i.PaidAtUtc
                })
                .ToListAsync(ct);

            return new PagedResult<InvoiceDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
