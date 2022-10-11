using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class AddTokenReferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Tokens",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "Token0TokenId",
                table: "TokenPairValues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Token1TokenId",
                table: "TokenPairValues",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_Token0TokenId",
                table: "TokenPairValues",
                column: "Token0TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_Token1TokenId",
                table: "TokenPairValues",
                column: "Token1TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenPairValues_Tokens_Token0TokenId",
                table: "TokenPairValues",
                column: "Token0TokenId",
                principalTable: "Tokens",
                principalColumn: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenPairValues_Tokens_Token1TokenId",
                table: "TokenPairValues",
                column: "Token1TokenId",
                principalTable: "Tokens",
                principalColumn: "TokenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairValues_Tokens_Token0TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairValues_Tokens_Token1TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_Token0TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_Token1TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "Token0TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "Token1TokenId",
                table: "TokenPairValues");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tokens",
                newName: "Title");
        }
    }
}
