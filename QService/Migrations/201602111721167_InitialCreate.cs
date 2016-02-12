namespace QService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExchangeBoards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Securities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ticker = c.String(),
                        Code = c.String(),
                        Name = c.String(),
                        StepPrice = c.Decimal(precision: 18, scale: 2),
                        ExchangeBoard = c.String(),
                        ExchangeBoard_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExchangeBoards", t => t.ExchangeBoard_Id)
                .Index(t => t.ExchangeBoard_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Securities", "ExchangeBoard_Id", "dbo.ExchangeBoards");
            DropIndex("dbo.Securities", new[] { "ExchangeBoard_Id" });
            DropTable("dbo.Securities");
            DropTable("dbo.ExchangeBoards");
        }
    }
}
