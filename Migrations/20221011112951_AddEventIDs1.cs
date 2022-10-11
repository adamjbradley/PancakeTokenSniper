using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class AddEventIDs1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TokenEvents_EventId",
                table: "TokenEvents",
                column: "EventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokenEvents_EventId",
                table: "TokenEvents");
        }
    }
}
