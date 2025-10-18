using Application.Common;
using Application.DTOs;
using Application.IServices;
using AutoMapper;
using Domain.Lijan;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public sealed class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDateTimeProvider _clock;
        private readonly IMapper _mapper;

        public SubscriptionService(ApplicationDbContext db, IDateTimeProvider clock, IMapper mapper)
        {
            _db = db;
            _clock = clock;
            _mapper = mapper;
        }

        // داخل Application.Services.SubscriptionService
        public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request, CancellationToken ct = default)
        {
            // *** 1) التحقق من الخطة والمستأجر ***
            var plan = await _db.Set<Plan>()
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId && p.IsActive && !p.IsDeleted, ct)
                ?? throw new InvalidOperationException("Plan not found or inactive.");

            var tenant = await _db.Set<Tenant>()
                .FirstOrDefaultAsync(t => t.TenantId == request.TenantId && !t.IsDeleted, ct)
                ?? throw new InvalidOperationException("Tenant not found.");

            // *** 2) كوبون (اختياري) ***
            Coupon? coupon = null;
            decimal discountPct = 0m;
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                coupon = await _db.Set<Coupon>().FirstOrDefaultAsync(c =>
                    c.CouponCode == request.CouponCode && c.IsActive && !c.IsDeleted
                    && (c.ValidFromUtc == null || c.ValidFromUtc <= _clock.UtcNow)
                    && (c.ValidToUtc == null || c.ValidToUtc >= _clock.UtcNow), ct)
                    ?? throw new InvalidOperationException("Invalid coupon.");

                if (coupon.MaxRedemptions.HasValue)
                {
                    var used = await _db.Set<Subscription>().CountAsync(s => s.CouponId == coupon.CouponId && !s.IsDeleted, ct);
                    if (used >= coupon.MaxRedemptions.Value)
                        throw new InvalidOperationException("Coupon redemption limit reached.");
                }
                discountPct = coupon.DiscountPercent;
            }

            // *** 3) VAT ***
            var vatPercent = await _db.Set<Vatrate>()
                .Where(v => v.CountryCode == tenant.CountryCode)
                .Select(v => v.VatPercent)
                .FirstOrDefaultAsync(ct);

            // *** 4) إنشاء الاشتراك ***
            var start = _clock.UtcNow;
            var end = start.AddMonths(request.MonthsCount);

            var sub = new Subscription
            {
                TenantId = tenant.TenantId,
                PlanId = plan.PlanId,
                MonthsCount = request.MonthsCount,
                AutoRenew = request.AutoRenew,
                SubStatusid = 1, // Active
                StartDateUtc = start,
                EndDateUtc = end,
                NextBillingUtc = request.AutoRenew ? end : null,
                CouponId = coupon?.CouponId,
                IsDeleted = false
            };

            _db.Add(sub);
            await _db.SaveChangesAsync(ct);

            // *** 5) حساب المبالغ وإنشاء الفاتورة ***
            var (subtotal, discount, tax) = PricingService.Compute(plan, request.MonthsCount, vatPercent, discountPct);
            var total = subtotal - discount + tax;

            var invoice = new Invoice
            {
                SubscriptionId = sub.SubscriptionId,
                InvoiceNumber = $"INV-{sub.SubscriptionId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                CurrencyCode = "SAR",
                AmountSubtotal = subtotal,
                DiscountAmount = discount,
                TaxAmount = tax,
                AmountTotal = total,         // تخزينها أيضاً داخل الجدول
                InvoiceStatusid = 1,         // Pending
                IssueDateUtc = _clock.UtcNow,
                DueDateUtc = _clock.UtcNow.AddDays(7),
                IsDeleted = false
            };

            _db.Add(invoice);
            await _db.SaveChangesAsync(ct);

            // *** 6) بناء DTO ***
            var dto = new SubscriptionDto
            {
                SubscriptionId = sub.SubscriptionId,
                TenantId = sub.TenantId,
                PlanId = sub.PlanId,
                MonthsCount = sub.MonthsCount,
                AutoRenew = sub.AutoRenew,
                SubStatusid = sub.SubStatusid,
                StartDateUtc = sub.StartDateUtc,
                EndDateUtc = sub.EndDateUtc,
                CouponId = sub.CouponId,
                Invoice = new InvoiceDto
                {
                    InvoiceId = invoice.InvoiceId,
                    SubscriptionId = invoice.SubscriptionId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CurrencyCode = invoice.CurrencyCode,
                    AmountSubtotal = invoice.AmountSubtotal,
                    DiscountAmount = invoice.DiscountAmount,
                    TaxAmount = invoice.TaxAmount,
                    AmountTotal = total,
                    InvoiceStatusid = invoice.InvoiceStatusid,
                    IssueDateUtc = invoice.IssueDateUtc,
                    DueDateUtc = invoice.DueDateUtc,
                    PaidAtUtc = invoice.PaidAtUtc
                }
            };

            return dto;
        }

        public async Task<bool> RenewAsync(long subscriptionId, CancellationToken ct = default)
        {
            var sub = await _db.Set<Subscription>().FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId && !s.IsDeleted, ct);
            if (sub == null) return false;

            // نقرأ الخطة
            var plan = await _db.Set<Plan>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlanId == sub.PlanId && p.IsActive && !p.IsDeleted, ct)
                ?? throw new InvalidOperationException("Plan not found or inactive.");

            // نقرأ المستأجر لمعرفة VAT
            var tenant = await _db.Set<Tenant>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == sub.TenantId && !t.IsDeleted, ct)
                ?? throw new InvalidOperationException("Tenant not found.");

            var vatPercent = await _db.Set<Vatrate>()
                .Where(v => v.CountryCode == tenant.CountryCode)
                .Select(v => v.VatPercent)
                .FirstOrDefaultAsync(ct);

            // نحدّد التاريخ الجديد
            sub.StartDateUtc = _clock.UtcNow;
            sub.EndDateUtc = sub.StartDateUtc.AddMonths(sub.MonthsCount);
            sub.NextBillingUtc = sub.AutoRenew ? sub.EndDateUtc : null;
            sub.SubStatusid = 1; // Active

            await _db.SaveChangesAsync(ct);

            // حساب الفاتورة الجديدة (بدون كوبون أو بنفس الـ CouponId إن أردت إعادة استخدامه)
            var (subtotal, discount, tax) = PricingService.Compute(plan, sub.MonthsCount, vatPercent, 0m);
            var total = subtotal - discount + tax;

            var invoice = new Invoice
            {
                SubscriptionId = sub.SubscriptionId,
                InvoiceNumber = $"INV-{sub.SubscriptionId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                CurrencyCode = "SAR",
                AmountSubtotal = subtotal,
                DiscountAmount = discount,
                TaxAmount = tax,
                AmountTotal = total,
                InvoiceStatusid = 1, // Pending
                IssueDateUtc = _clock.UtcNow,
                DueDateUtc = _clock.UtcNow.AddDays(7),
                IsDeleted = false
            };

            _db.Add(invoice);
            await _db.SaveChangesAsync(ct);

            return true;
        }
        public async Task<PagedResult<SubscriptionListItemDto>> AdminSearchAsync(
    long? tenantId, long? planId, int? statusId,
    DateTime? fromUtc, DateTime? toUtc,
    int page, int pageSize, CancellationToken ct = default)
        {
            var query =
                from s in _db.Set<Subscription>().AsNoTracking()
                join t in _db.Set<Tenant>().AsNoTracking() on s.TenantId equals t.TenantId into gt
                from t in gt.DefaultIfEmpty()
                join p in _db.Set<Plan>().AsNoTracking() on s.PlanId equals p.PlanId into gp
                from p in gp.DefaultIfEmpty()
                where !s.IsDeleted
                select new SubscriptionListItemDto
                {
                    SubscriptionId = s.SubscriptionId,
                    TenantId = s.TenantId,
                    TenantName = t != null ? t.TenantName : "",
                    TenantEmail = t != null ? t.TenantEmail : "",
                    PlanId = s.PlanId,
                    PlanNameAr = p != null ? p.PlanNameAr : "",
                    PlanNameEn = p != null ? p.PlanNameEn : "",
                    MonthsCount = s.MonthsCount,
                    AutoRenew = s.AutoRenew,
                    SubStatusid = s.SubStatusid,
                    StartDateUtc = s.StartDateUtc,
                    EndDateUtc = s.EndDateUtc,
                    NextBillingUtc = s.NextBillingUtc,
                    CouponId = s.CouponId
                };

            if (tenantId.HasValue) query = query.Where(x => x.TenantId == tenantId.Value);
            if (planId.HasValue) query = query.Where(x => x.PlanId == planId.Value);
            if (statusId.HasValue) query = query.Where(x => x.SubStatusid == statusId.Value);
            if (fromUtc.HasValue) query = query.Where(x => x.StartDateUtc >= fromUtc.Value);
            if (toUtc.HasValue) query = query.Where(x => x.StartDateUtc <= toUtc.Value);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.StartDateUtc)
                .ThenByDescending(x => x.SubscriptionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<SubscriptionListItemDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

    }
}
