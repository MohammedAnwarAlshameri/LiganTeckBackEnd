using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Lijan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.EntityConfigurations.Lijan
{
   
    public class VatrateConfig : IEntityTypeConfiguration<Vatrate>
    {
        public void Configure(EntityTypeBuilder<Vatrate> entity)
        {
            entity.ToTable("VATRate");

            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength();
            entity.Property(e => e.VatPercent).HasColumnType("decimal(5, 4)");
        }
    }
}
