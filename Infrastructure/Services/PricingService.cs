using Domain.Lijan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public static class PricingService
    {
        // vatPercent: مثال 15.0000 => 15%
        public static (decimal subtotal, decimal discount, decimal tax) Compute(Plan plan, int months, decimal vatPercent, decimal discountPercent)
        {
            var subtotal = plan.MonthlyPrice * months;
            var discount = Math.Round(subtotal * (discountPercent / 100m), 2);
            var taxable = subtotal - discount;
            var tax = Math.Round(taxable * (vatPercent / 100m), 2);
            return (subtotal, discount, tax);
        }
    }
}
