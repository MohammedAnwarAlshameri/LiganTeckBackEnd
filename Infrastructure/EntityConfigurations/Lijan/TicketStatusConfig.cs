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
   
    public class TicketStatusConfig : IEntityTypeConfiguration<TicketStatus>
    {
        public void Configure(EntityTypeBuilder<TicketStatus> entity)
        {
            entity .ToTable("TicketStatus");
            entity.HasKey(e => e.TicketStatusid);
            entity.Property(e => e.TicketStatusName).HasMaxLength(50);
            entity.Property(e => e.TicketStatusdescription).HasMaxLength(200);
            entity.Property(e => e.TicketStatusid).ValueGeneratedOnAdd();
        }
    }
}
