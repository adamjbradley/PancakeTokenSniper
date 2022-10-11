using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Globalization;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class LiquidityEventTweaks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiquidityEvents_TokenPairs_TokenPairId",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "LiquidityEvents");

            migrationBuilder.AlterColumn<int>(
                name: "TokenPairId",
                table: "LiquidityEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Amount",
                table: "LiquidityEvents",
                type: "numeric",
                nullable: false,
                defaultValue: long.Parse("0", NumberFormatInfo.InvariantInfo));

            migrationBuilder.AddColumn<string>(
                name: "PairAddress",
                table: "LiquidityEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token0",
                table: "LiquidityEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token1",
                table: "LiquidityEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidityEvents_TokenPairs_TokenPairId",
                table: "LiquidityEvents",
                column: "TokenPairId",
                principalTable: "TokenPairs",
                principalColumn: "TokenPairId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiquidityEvents_TokenPairs_TokenPairId",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "PairAddress",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "Token0",
                table: "LiquidityEvents");

            migrationBuilder.DropColumn(
                name: "Token1",
                table: "LiquidityEvents");

            migrationBuilder.AlterColumn<int>(
                name: "TokenPairId",
                table: "LiquidityEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TokenId",
                table: "LiquidityEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "LiquidityEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidityEvents_TokenPairs_TokenPairId",
                table: "LiquidityEvents",
                column: "TokenPairId",
                principalTable: "TokenPairs",
                principalColumn: "TokenPairId");
        }
    }
}
