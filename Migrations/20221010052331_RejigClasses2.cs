using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class RejigClasses2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairs_Tokens_Token0TokenId",
                table: "TokenPairs");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairs_Tokens_Token1TokenId",
                table: "TokenPairs");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairs_Token0TokenId",
                table: "TokenPairs");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairs_Token1TokenId",
                table: "TokenPairs");

            migrationBuilder.DropColumn(
                name: "Token0TokenId",
                table: "TokenPairs");

            migrationBuilder.DropColumn(
                name: "Token1TokenId",
                table: "TokenPairs");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TokenPairs",
                newName: "Token1");

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "TokenPairs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token0",
                table: "TokenPairs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "TokenPairs");

            migrationBuilder.DropColumn(
                name: "Token0",
                table: "TokenPairs");

            migrationBuilder.RenameColumn(
                name: "Token1",
                table: "TokenPairs",
                newName: "Title");

            migrationBuilder.AddColumn<int>(
                name: "Token0TokenId",
                table: "TokenPairs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Token1TokenId",
                table: "TokenPairs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairs_Token0TokenId",
                table: "TokenPairs",
                column: "Token0TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairs_Token1TokenId",
                table: "TokenPairs",
                column: "Token1TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenPairs_Tokens_Token0TokenId",
                table: "TokenPairs",
                column: "Token0TokenId",
                principalTable: "Tokens",
                principalColumn: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenPairs_Tokens_Token1TokenId",
                table: "TokenPairs",
                column: "Token1TokenId",
                principalTable: "Tokens",
                principalColumn: "TokenId");
        }
    }
}
