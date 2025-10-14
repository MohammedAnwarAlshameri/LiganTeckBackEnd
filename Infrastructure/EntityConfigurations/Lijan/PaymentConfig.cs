using Domain.Lijan;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.EntityConfigurations.Lijan
{
    
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> entity)
        {
            entity.ToTable("Payment");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("SAR")
                .IsFixedLength();
            entity.Property(e => e.FailureCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FailureMessage).HasMaxLength(400);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PaidAtUtc).HasPrecision(0);
            entity.Property(e => e.ProviderRef).HasMaxLength(100);

        }
    }
}
