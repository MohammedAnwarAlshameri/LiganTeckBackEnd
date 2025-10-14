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
  
    public class PlanConfig : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> entity)
        {
            entity.HasKey(e => e.PlanId).HasName("PK_Plan");

            entity.HasIndex(e => e.PlanCode, "UQ_Plan_Code").IsUnique();

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PlanCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PlanDetails).HasMaxLength(500);
            entity.Property(e => e.PlanNameAr).HasMaxLength(200);
            entity.Property(e => e.PlanNameEn).HasMaxLength(200);

        }
    }
}
