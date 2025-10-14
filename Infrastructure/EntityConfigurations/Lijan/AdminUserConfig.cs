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
   
    public class AdminUserConfig : IEntityTypeConfiguration<AdminUser>
    {
        public void Configure(EntityTypeBuilder<AdminUser> entity)
        {
            entity.HasKey(e => e.AdminUserId).HasName("PK__AdminUse__02DDFE7B06A9BCC7");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
