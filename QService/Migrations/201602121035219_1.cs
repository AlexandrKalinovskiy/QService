namespace QService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Securities", "ExchangeBoard");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Securities", "ExchangeBoard", c => c.String());
        }
    }
}
