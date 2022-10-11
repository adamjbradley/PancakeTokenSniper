using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class RejigClasses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairValues_Tokens_Token0TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairValues_Tokens_Token1TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_TokenPairs_TokenPairId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_TokenPairId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_Token0TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_Token1TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_TokenPairId",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "TokenPairId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "Token0TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "Token1TokenId",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "LiquidityEvents");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tokens",
                newName: "Symbol");

            migrationBuilder.AlterColumn<bool>(
                name: "Blocked",
                table: "Tokens",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

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

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "LiquidityEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_TokenPairId",
                table: "TokenPairValues",
                column: "TokenPairId");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairs_Tokens_Token0TokenId",
                table: "TokenPairs");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenPairs_Tokens_Token1TokenId",
                table: "TokenPairs");

            migrationBuilder.DropIndex(
                name: "IX_TokenPairValues_TokenPairId",
                table: "TokenPairValues");

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

            migrationBuilder.DropColumn(
                name: "Value",
                table: "LiquidityEvents");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "Tokens",
                newName: "Name");

            migrationBuilder.AlterColumn<bool>(
                name: "Blocked",
                table: "Tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenPairId",
                table: "Tokens",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "LiquidityEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LiquidityEvents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenPairId",
                table: "Tokens",
                column: "TokenPairId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_Token0TokenId",
                table: "TokenPairValues",
                column: "Token0TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_Token1TokenId",
                table: "TokenPairValues",
                column: "Token1TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_TokenPairId",
                table: "TokenPairValues",
                column: "TokenPairId",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_TokenPairs_TokenPairId",
                table: "Tokens",
                column: "TokenPairId",
                principalTable: "TokenPairs",
                principalColumn: "TokenPairId");
        }
    }
}
