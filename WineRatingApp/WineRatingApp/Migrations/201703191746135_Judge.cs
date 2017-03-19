namespace WineRatingApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Judge : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Wines", "WineProducerId", "dbo.WineProducers");
            DropIndex("dbo.Wines", new[] { "WineProducerId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.Wines", "WineProducerId");
            AddForeignKey("dbo.Wines", "WineProducerId", "dbo.WineProducers", "WineProducerId", cascadeDelete: true);
        }
    }
}
