using Microsoft.EntityFrameworkCore;
using Domain.Lijan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public interface IApplicationDbContext
    {

       DbSet<AuditLog> AuditLogs { get; }

         DbSet<Coupon> Coupons { get; }

         DbSet<Invoice> Invoices { get; }

        DbSet<InvoiceStatus> InvoiceStatuses { get; }

        DbSet<Payment> Payments { get; }

         DbSet<PaymentMethod> PaymentMethods { get; }

        DbSet<PaymentStatus> PaymentStatuses { get; }

        DbSet<Plan> Plans { get; }

         DbSet<SubscribtionStatus> SubscribtionStatuses { get; }

         DbSet<Subscription> Subscriptions { get; }

         DbSet<Tenant> Tenants { get;    }

         DbSet<TenantStatus> TenantStatuses { get; }

        DbSet<Ticket> Tickets { get; }

        DbSet<TicketChat> TicketChats { get;  }

         DbSet<TicketStatus> TicketStatuses { get; }

         DbSet<Vatrate> Vatrates { get; }

        DbSet<AdminUser> AdminUser { get; }








    }
}
