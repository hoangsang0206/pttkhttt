namespace STech.IdentityMigration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "UserFullName", c => c.String());
            AddColumn("dbo.AspNetUsers", "Gender", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Gender");
            DropColumn("dbo.AspNetUsers", "UserFullName");
        }
    }
}
