using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class ConcurrencyIssues3BadIdea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TokenPairValues");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TokenEvents");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "LiquidityEvents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "xmin",
                table: "TokenPairValues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "xmin",
                table: "TokenEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "xmin",
                table: "LiquidityEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
