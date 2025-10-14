using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Lijan;

namespace Infrastructure.EntityConfigurations.Lijan
{
    
    public class InvoiceConfig : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> entity)
        {
            entity.ToTable("Invoice");

            entity.HasIndex(e => e.InvoiceNumber, "UQ_Invoice_Number").IsUnique();

            entity.Property(e => e.AmountSubtotal).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.AmountTotal)
                .HasComputedColumnSql("(([AmountSubtotal]-[DiscountAmount])+[TaxAmount])", true)
                .HasColumnType("decimal(16, 2)");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("SAR")
                .IsFixedLength();
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.DueDateUtc).HasPrecision(0);
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.IssueDateUtc).HasPrecision(0);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PaidAtUtc).HasPrecision(0);
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(14, 2)");

        }
    }
}
