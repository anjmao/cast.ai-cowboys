using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cowboys.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cowboys",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    health = table.Column<int>(type: "integer", nullable: false),
                    damage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cowboys", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    eventtime = table.Column<DateTime>(name: "event_time", type: "timestamp with time zone", nullable: false),
                    eventtype = table.Column<string>(name: "event_type", type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    gameid = table.Column<Guid>(name: "game_id", type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_events_game_game_id",
                        column: x => x.gameid,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "in_game_cowboys",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    gameid = table.Column<Guid>(name: "game_id", type: "uuid", nullable: false),
                    health = table.Column<int>(type: "integer", nullable: false),
                    damage = table.Column<int>(type: "integer", nullable: false),
                    isselected = table.Column<bool>(name: "is_selected", type: "boolean", nullable: false),
                    isready = table.Column<bool>(name: "is_ready", type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_in_game_cowboys", x => new { x.gameid, x.name });
                    table.ForeignKey(
                        name: "fk_in_game_cowboys_game_game_id",
                        column: x => x.gameid,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "cowboys",
                columns: new[] { "name", "damage", "health" },
                values: new object[,]
                {
                    { "Bill", 2, 8 },
                    { "John", 1, 10 },
                    { "Peter", 3, 5 },
                    { "Philip", 1, 15 },
                    { "Sam", 1, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_cowboys_name",
                table: "cowboys",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_id",
                table: "game",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_events_game_id",
                table: "game_events",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_in_game_cowboys_game_id_name",
                table: "in_game_cowboys",
                columns: new[] { "game_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cowboys");

            migrationBuilder.DropTable(
                name: "game_events");

            migrationBuilder.DropTable(
                name: "in_game_cowboys");

            migrationBuilder.DropTable(
                name: "game");
        }
    }
}
