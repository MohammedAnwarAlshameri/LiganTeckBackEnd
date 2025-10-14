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
   
    public class TicketChatConfig : IEntityTypeConfiguration<TicketChat>
    {
        public void Configure(EntityTypeBuilder<TicketChat> entity)
        {
            entity.HasKey(e => e.ChatId);

            entity.ToTable("TicketChat");

            entity.Property(e => e.AdminText).HasMaxLength(300);
            entity.Property(e => e.AdminTextAtUtc).HasPrecision(0);
            entity.Property(e => e.TenantText).HasMaxLength(300);
            entity.Property(e => e.TenantTextAtUtc).HasPrecision(0);
        }
    }
}
