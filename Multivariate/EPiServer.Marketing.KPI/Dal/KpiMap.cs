﻿using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.KPI.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    public class KpiMap : EntityTypeConfiguration<Kpi>
    {
        public KpiMap()
        {
            this.ToTable("tblKeyPerformaceIndicator");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.Name)
                .IsRequired();

            this.Property(m => m.Weight)
                .IsOptional();

            this.Property(m => m.Value)
                .IsRequired();

            this.Property(m => m.LandingPage)
                .IsRequired();

            this.Property(m => m.RunAt)
                .IsRequired();

            this.Property(m => m.ClientScripts)
                .IsOptional();

            this.Property(m => m.CreatedDate)
                .IsRequired();

            this.Property(m => m.ModifiedDate)
                .IsRequired();

        }
    }
}
