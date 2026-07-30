using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FF_DataDB.Migrations
{
    /// <inheritdoc />
    public partial class AddFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SourceItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastFavoriteAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    SourceSecretId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_SourceItems_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "SourceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_SourceSecrets_SourceSecretId",
                        column: x => x.SourceSecretId,
                        principalTable: "SourceSecrets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_SourceItemId",
                table: "Favorites",
                column: "SourceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_SourceSecretId",
                table: "Favorites",
                column: "SourceSecretId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_SourceItemId",
                table: "Favorites",
                columns: new[] { "UserId", "SourceItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");
        }
    }
}
