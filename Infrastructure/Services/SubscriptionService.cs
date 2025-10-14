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

        public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request, CancellationToken ct = default)
        {
            // 1) التحقق من الخطة والمستأجر
            var plan = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.PlanId == request.PlanId && p.IsActive && !p.IsDeleted, ct)
                       ?? throw new InvalidOperationException("Plan not found or inactive.");

            var tenant = await _db.Set<Tenant>().FirstOrDefaultAsync(t => t.TenantId == request.TenantId && !t.IsDeleted, ct)
                         ?? throw new InvalidOperationException("Tenant not found.");

            // 2) كوبون (اختياري)
            Coupon? coupon = null;
            decimal discountPct = 0;
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

            // 3) VAT حسب دولة المستأجر
            var vatPercent = await _db.Set<Vatrate>()
                .Where(v => v.CountryCode == tenant.CountryCode)
                .Select(v => v.VatPercent)
                .FirstOrDefaultAsync(ct);

            // 4) إنشاء الاشتراك
            var start = _clock.UtcNow;
            var end = start.AddMonths(request.MonthsCount);

            var sub = new Subscription
            {
                TenantId = tenant.TenantId,
                PlanId = plan.PlanId,
                MonthsCount = request.MonthsCount,
                AutoRenew = request.AutoRenew,
                SubStatusid = 1, // Active/Pending Activation
                StartDateUtc = start,
                EndDateUtc = end,
                NextBillingUtc = request.AutoRenew ? end : null,
                CouponId = coupon?.CouponId,
                IsDeleted = false
            };
            _db.Add(sub);
            await _db.SaveChangesAsync(ct);

            // 5) حساب الفاتورة (وبناء الكيان)
            var (subtotal, discount, tax) = PricingService.Compute(plan, request.MonthsCount, vatPercent, discountPct);
            var amountTotal = subtotal - discount + tax;

            var invoice = new Invoice
            {
                SubscriptionId = sub.SubscriptionId,
                InvoiceNumber = $"INV-{sub.SubscriptionId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                CurrencyCode = "SAR",
                AmountSubtotal = subtotal,
                DiscountAmount = discount,
                TaxAmount = tax,
                InvoiceStatusid = 1, // Pending
                IssueDateUtc = _clock.UtcNow,
                DueDateUtc = _clock.UtcNow.AddDays(7),
                IsDeleted = false
            };
            _db.Add(invoice);
            await _db.SaveChangesAsync(ct);

            // 6) بناء DTO عبر المابر
            var dto = _mapper.Map<SubscriptionDto>(sub);
            dto.Invoice = _mapper.Map<InvoiceDto>(invoice);
            dto.Invoice.AmountTotal = amountTotal; // نضمنها بالقيمة الصحيحة
            return dto;
        }

        public async Task<bool> RenewAsync(long subscriptionId, CancellationToken ct = default)
        {
            var sub = await _db.Set<Subscription>().FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId && !s.IsDeleted, ct);
            if (sub == null) return false;

            sub.StartDateUtc = _clock.UtcNow;
            sub.EndDateUtc = sub.StartDateUtc.AddMonths(sub.MonthsCount);
            sub.NextBillingUtc = sub.AutoRenew ? sub.EndDateUtc : null;
            sub.SubStatusid = 1; // Active

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
