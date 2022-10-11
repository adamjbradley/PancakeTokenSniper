using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokenPairs",
                columns: table => new
                {
                    TokenPairId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenPairs", x => x.TokenPairId);
                });

            migrationBuilder.CreateTable(
                name: "LiquidityEvents",
                columns: table => new
                {
                    LiquidityEventId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    TokenId = table.Column<int>(type: "integer", nullable: false),
                    TokenPairId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidityEvents", x => x.LiquidityEventId);
                    table.ForeignKey(
                        name: "FK_LiquidityEvents_TokenPairs_TokenPairId",
                        column: x => x.TokenPairId,
                        principalTable: "TokenPairs",
                        principalColumn: "TokenPairId");
                });

            migrationBuilder.CreateTable(
                name: "TokenPairValues",
                columns: table => new
                {
                    TokenPairValueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    TokenPairId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenPairValues", x => x.TokenPairValueId);
                    table.ForeignKey(
                        name: "FK_TokenPairValues_TokenPairs_TokenPairId",
                        column: x => x.TokenPairId,
                        principalTable: "TokenPairs",
                        principalColumn: "TokenPairId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    TokenPairId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_Tokens_TokenPairs_TokenPairId",
                        column: x => x.TokenPairId,
                        principalTable: "TokenPairs",
                        principalColumn: "TokenPairId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityEvents_TokenPairId",
                table: "LiquidityEvents",
                column: "TokenPairId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenPairValues_TokenPairId",
                table: "TokenPairValues",
                column: "TokenPairId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_TokenPairId",
                table: "Tokens",
                column: "TokenPairId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiquidityEvents");

            migrationBuilder.DropTable(
                name: "TokenPairValues");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "TokenPairs");
        }
    }
}
