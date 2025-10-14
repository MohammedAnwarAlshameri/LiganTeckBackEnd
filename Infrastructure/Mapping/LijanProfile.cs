using Application.DTOs;
using AutoMapper;
using Domain.Lijan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mapping
{
    public class LijanProfile : Profile
    {
        public LijanProfile()
        {
            // Plan -> PlanDto
            CreateMap<Plan, PlanDto>().ReverseMap();

            // Invoice -> InvoiceDto
            CreateMap<Invoice, InvoiceDto>()
                // لو AmountTotal مش محمّل من SQL، نحسبها محليًا (Subtotal - Discount + Tax)
                .ForMember(d => d.AmountTotal,
                    opt => opt.MapFrom(s => (decimal?)(s.AmountSubtotal - s.DiscountAmount + s.TaxAmount))).ReverseMap();

            // Subscription -> SubscriptionDto (بدون الفاتورة)
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(d => d.Invoice, opt => opt.Ignore()).ReverseMap(); // نملأها يدويًا عند الحاجة
        }
    }
}
