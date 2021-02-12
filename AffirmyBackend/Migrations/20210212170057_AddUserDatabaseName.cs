using Microsoft.EntityFrameworkCore.Migrations;

namespace AffirmyBackend.Migrations
{
    public partial class AddUserDatabaseName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserDatabaseName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserDatabaseName",
                table: "AspNetUsers");
        }
    }
}
