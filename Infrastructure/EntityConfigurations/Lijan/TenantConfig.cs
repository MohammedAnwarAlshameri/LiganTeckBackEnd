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
   
    public class TenantConfig : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> entity)
        {
            entity.ToTable("Tenant");

            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .HasDefaultValue("SA")
                .IsFixedLength();
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenantEmail).HasMaxLength(200);
            entity.Property(e => e.TenantKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.TenantName).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.TenantPassword).HasMaxLength(200);
            entity.Property(e => e.TenantStatusid).HasDefaultValue(1L);

        }
    }
}
