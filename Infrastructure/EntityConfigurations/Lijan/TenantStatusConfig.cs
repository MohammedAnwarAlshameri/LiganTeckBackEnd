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
   

    public class TenantStatusConfig : IEntityTypeConfiguration<TenantStatus>
    {
        public void Configure(EntityTypeBuilder<TenantStatus> entity)
        {
            entity 
                .ToTable("TenantStatus");

            entity.HasKey(e => e.TenantStatusid);
            entity.Property(e => e.TenantStatusName).HasMaxLength(50);
            entity.Property(e => e.TenantStatusdescription).HasMaxLength(200);
            entity.Property(e => e.TenantStatusid).ValueGeneratedOnAdd();
            
        }
    }
}
