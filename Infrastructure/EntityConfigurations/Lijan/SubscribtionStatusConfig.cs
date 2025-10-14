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
    
    public class SubscribtionStatusConfig : IEntityTypeConfiguration<SubscribtionStatus>
    {
        public void Configure(EntityTypeBuilder<SubscribtionStatus> entity)
        {
            entity
              .HasNoKey()
              .ToTable("SubscribtionStatus");

            entity.Property(e => e.SubStatusName).HasMaxLength(50);
            entity.Property(e => e.SubStatusdescription).HasMaxLength(200);
            entity.Property(e => e.SubStatusid).ValueGeneratedOnAdd();
        }
    }
}
