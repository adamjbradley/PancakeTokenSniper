using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class AddEventIDs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "TokenPairValues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventId",
                table: "TokenEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventId",
                table: "LiquidityEvents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_Address",
                table: "TokenPairValues",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityEvents_EventId",
                table: "LiquidityEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityEvents_PairAddress",
                table: "LiquidityEvents",
                column: "PairAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_Address",
                table: "TokenPairValues");

            migrationBuilder.DropIndex(
                name: "IX_LiquidityEvents_EventId",
                table: "LiquidityEvents");

            migrationBuilder.DropIndex(
                name: "IX_LiquidityEvents_PairAddress",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "TokenEvents");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "LiquidityEvents");
        }
    }
}
