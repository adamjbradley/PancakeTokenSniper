using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class MoreTokenStateInfo5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Value",
                table: "TokenPairValues",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "FiftyTwoWeekHigh",
                table: "TokenPairValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FiftyTwoWeekLow",
                table: "TokenPairValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "High",
                table: "TokenPairValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Low",
                table: "TokenPairValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiftyTwoWeekHigh",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "FiftyTwoWeekLow",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "High",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "Low",
                table: "TokenPairValues");

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "TokenPairValues",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
