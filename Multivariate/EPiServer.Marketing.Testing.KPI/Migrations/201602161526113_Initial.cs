namespace EPiServer.Marketing.Testing.KPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblKeyPerformaceIndicator",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Weight = c.Int(nullable: false),
                        Value = c.String(nullable: false),
                        ParticipationPercentage = c.Int(nullable: false),
                        ConversionPage = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblKeyPerformaceIndicator");
        }
    }
}
