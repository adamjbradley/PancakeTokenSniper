using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BscTokenSniper.Migrations
{
    public partial class AddTokenStates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokenEvents",
                columns: table => new
                {
                    TokenEventId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    TokenPairId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenEvents", x => x.TokenEventId);
                    table.ForeignKey(
                        name: "FK_TokenEvents_TokenPairs_TokenPairId",
                        column: x => x.TokenPairId,
                        principalTable: "TokenPairs",
                        principalColumn: "TokenPairId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenEvents_Address",
                table: "TokenEvents",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_TokenEvents_TokenPairId",
                table: "TokenEvents",
                column: "TokenPairId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenEvents");
        }
    }
}
