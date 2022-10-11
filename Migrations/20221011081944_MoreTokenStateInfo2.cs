using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class MoreTokenStateInfo2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventResult",
                table: "TokenEvents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventResult",
                table: "TokenEvents");
        }
    }
}
