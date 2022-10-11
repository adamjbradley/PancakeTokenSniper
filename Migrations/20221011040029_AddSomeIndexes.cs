using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class AddSomeIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tokens_Address",
                table: "Tokens",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairs_Address",
                table: "TokenPairs",
                column: "Address");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_Address",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairs_Address",
                table: "TokenPairs");
        }
    }
}
