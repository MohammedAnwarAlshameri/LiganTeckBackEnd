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
   
    public class InvoiceStatusConfig : IEntityTypeConfiguration<InvoiceStatus>
    {
        public void Configure(EntityTypeBuilder<InvoiceStatus> entity)
        {
            entity
                .HasNoKey()
                .ToTable("InvoiceStatus");

            //entity.HasKey(e => e.InvoiceStatusid);
            entity.Property(e => e.InvoiceStatusName).HasMaxLength(50);
            entity.Property(e => e.InvoiceStatusdescription).HasMaxLength(200);
            entity.Property(e => e.InvoiceStatusid).ValueGeneratedOnAdd();
        }
    }
}
