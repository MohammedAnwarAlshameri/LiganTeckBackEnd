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
   
    public class PaymentStatusConfig : IEntityTypeConfiguration<PaymentStatus>
    {
        public void Configure(EntityTypeBuilder<PaymentStatus> entity)
        {
            entity
                .HasNoKey()
                .ToTable("PaymentStatus");

            entity.Property(e => e.PaymentStatusName).HasMaxLength(50);
            entity.Property(e => e.PaymentStatusdescription).HasMaxLength(200);
            entity.Property(e => e.PaymentStatusid).ValueGeneratedOnAdd();

        }
    }
}
