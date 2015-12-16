﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Dal.Mappings
{
    public class MultivariateTestMap : EntityTypeConfiguration<MultivariateTest>
    {
        public MultivariateTestMap()
        {
            this.ToTable("tblMultivariateTest");

            this.HasKey(hk => hk.Id)
                .Property(hk => hk.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(m => m.Title)
                .HasMaxLength(255)
                .IsRequired();

            this.Property(m => m.Owner)
                .HasMaxLength(100)
                .IsRequired();

            this.Property(m => m.OriginalItemId)
                .IsRequired();

            this.Property(m => m.TestState)
                .IsOptional();

            this.Property(m => m.StartDate)
                .IsRequired();

            this.Property(m => m.EndDate)
                .IsRequired();

            this.Property(m => m.LastModifiedBy)
                .HasMaxLength(100)
                .IsOptional();

            this.Property(m => m.CreatedDate)
                .IsRequired();

            this.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
