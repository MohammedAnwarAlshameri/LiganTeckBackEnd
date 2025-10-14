using Microsoft.EntityFrameworkCore;
using Domain.Lijan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.EntityConfigurations.Lijan;
using Application.Common;
using System.Linq.Expressions;

namespace Infrastructure.DbContexts
{
    public  class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        private readonly IDateTimeProvider _clock;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options , ICurrentUser currentUser,
                                IDateTimeProvider clocks) : base(options)
        {
            _currentUser = currentUser;
            _clock = clocks;
        }

        #region "Lijan"
        public DbSet<AuditLog> AuditLogs { get; private set; }

        public DbSet<Coupon> Coupons { get; private set; }

        public  DbSet<Invoice> Invoices { get; private set; }

        public DbSet<InvoiceStatus> InvoiceStatuses { get; private set; }

        public  DbSet<Payment> Payments { get; private set; }

        public DbSet<PaymentMethod> PaymentMethods { get; private set; }

        public DbSet<PaymentStatus> PaymentStatuses { get; private set; }

        public DbSet<Plan> Plans { get; set; }

        public DbSet<SubscribtionStatus> SubscribtionStatuses { get; private set; }

        public DbSet<Subscription> Subscriptions { get; private set; }

        public DbSet<Tenant> Tenants { get; private set; }

        public DbSet<TenantStatus> TenantStatuses { get; private set; }

        public DbSet<Ticket> Tickets { get; private set; }

        public DbSet<TicketChat> TicketChats { get; private set; }

        public DbSet<TicketStatus> TicketStatuses { get; private set; }

        public DbSet<Vatrate> Vatrates { get; private set; }

        public DbSet<AdminUser> AdminUser { get; private set; }


        #endregion"LijanConfig"

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            #region "LijanConfig"
            modelBuilder.ApplyConfiguration(new AuditLogConfig());
            modelBuilder.ApplyConfiguration(new CouponConfig());
            modelBuilder.ApplyConfiguration(new InvoiceConfig());
            modelBuilder.ApplyConfiguration(new InvoiceStatusConfig());
            modelBuilder.ApplyConfiguration(new PaymentConfig());
            modelBuilder.ApplyConfiguration(new PaymentMethodConfig());
            modelBuilder.ApplyConfiguration(new PaymentStatusConfig());
            modelBuilder.ApplyConfiguration(new PlanConfig());
            modelBuilder.ApplyConfiguration(new SubscriptionConfig());
            modelBuilder.ApplyConfiguration(new SubscribtionStatusConfig());
            modelBuilder.ApplyConfiguration(new TenantConfig());
            modelBuilder.ApplyConfiguration(new TenantStatusConfig());
            modelBuilder.ApplyConfiguration(new TicketChatConfig());
            modelBuilder.ApplyConfiguration(new TicketConfig());
            modelBuilder.ApplyConfiguration(new TicketStatusConfig());
            modelBuilder.ApplyConfiguration(new VatrateConfig());
            modelBuilder.ApplyConfiguration(new AdminUserConfig());
            #endregion"LijanConfig"

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDeleted = entityType.FindProperty("IsDeleted");
                if (isDeleted != null && isDeleted.ClrType == typeof(bool))
                {
                    var param = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(param, "IsDeleted");
                    var cond = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(cond, param);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void StampAuditFields()
        {
            var now = _clock.UtcNow;

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                // Soft delete
                if (entry.State == EntityState.Deleted && entry.Properties.Any(p => p.Metadata.Name == "IsDeleted"))
                {
                    entry.State = EntityState.Modified;
                    entry.CurrentValues["IsDeleted"] = true;
                    entry.CurrentValues["ModifiedOn"] = now;
                    if (entry.Properties.Any(p => p.Metadata.Name == "ModifiedBy"))
                        entry.CurrentValues["ModifiedBy"] =  _currentUser.TenantId;
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    if (entry.Properties.Any(p => p.Metadata.Name == "CreatedOn"))
                        entry.CurrentValues["CreatedOn"] = now;

                    if (entry.Properties.Any(p => p.Metadata.Name == "CreatedBy"))
                        entry.CurrentValues["CreatedBy"] =_currentUser.TenantId;

                    if (entry.Properties.Any(p => p.Metadata.Name == "IsDeleted") && entry.CurrentValues["IsDeleted"] is null)
                        entry.CurrentValues["IsDeleted"] = false;
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Properties.Any(p => p.Metadata.Name == "ModifiedOn"))
                        entry.CurrentValues["ModifiedOn"] = now;

                    if (entry.Properties.Any(p => p.Metadata.Name == "ModifiedBy"))
                        entry.CurrentValues["ModifiedBy"] =  _currentUser.TenantId;
                }
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DefaultConnection");
            }
        }
    }
}
