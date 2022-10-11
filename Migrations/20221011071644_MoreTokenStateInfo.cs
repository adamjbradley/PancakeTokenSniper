using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class MoreTokenStateInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRejected",
                table: "TokenPairs",
                newName: "ToTrade");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "TokenEvents",
                newName: "WalletAddress");

            migrationBuilder.AddColumn<bool>(
                name: "Owned",
                table: "TokenPairs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "TokenPairs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BuyQuantity",
                table: "TokenEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "BuyValue",
                table: "TokenEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Wallet",
                table: "TokenEvents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owned",
                table: "TokenPairs");

            migrationBuilder.DropColumn(
                name: "State",
                table: "TokenPairs");

            migrationBuilder.DropColumn(
                name: "BuyQuantity",
                table: "TokenEvents");

            migrationBuilder.DropColumn(
                name: "BuyValue",
                table: "TokenEvents");

            migrationBuilder.DropColumn(
                name: "Wallet",
                table: "TokenEvents");

            migrationBuilder.RenameColumn(
                name: "ToTrade",
                table: "TokenPairs",
                newName: "IsRejected");

            migrationBuilder.RenameColumn(
                name: "WalletAddress",
                table: "TokenEvents",
                newName: "State");
        }
    }
}
