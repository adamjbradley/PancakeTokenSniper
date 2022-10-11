using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class MoreTokenStateInfo3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "TokenEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "TokenEvents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Success",
                table: "TokenEvents");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "TokenEvents");
        }
    }
}
