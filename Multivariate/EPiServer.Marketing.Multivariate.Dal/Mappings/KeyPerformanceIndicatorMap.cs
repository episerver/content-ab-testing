﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class KeyPerformanceIndicatorMap : EntityTypeConfiguration<KeyPerformanceIndicator>
    {
        public KeyPerformanceIndicatorMap()
        {
            this.ToTable("tblKeyPerformanceIndicator");

            this.HasKey(hk => hk.Id)
                .Property(hk => hk.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(m => m.KeyPerformanceIndicatorId)
                .IsOptional();

            this.HasRequired(m => m.MultivariateTest)
                .WithMany(m => m.KeyPerformanceIndicators)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
