using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamiHub.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema_20260626 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shoppinglists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FamilyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shoppinglists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shoppinglists_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shoppingitems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ListId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<double>(type: "double", nullable: false),
                    Unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBought = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shoppingitems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shoppingitems_shoppinglists_ListId",
                        column: x => x.ListId,
                        principalTable: "shoppinglists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shoppingitems_users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_shoppingitems_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -3,
                column: "Title",
                value: "Mua đồ ăn vặt");

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -2,
                column: "Title",
                value: "Chơi game 1 giờ");

            migrationBuilder.CreateIndex(
                name: "IX_shoppingitems_BuyerId",
                table: "shoppingitems",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_shoppingitems_CreatedByUserId",
                table: "shoppingitems",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_shoppingitems_ListId",
                table: "shoppingitems",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_shoppinglists_FamilyId",
                table: "shoppinglists",
                column: "FamilyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shoppingitems");

            migrationBuilder.DropTable(
                name: "shoppinglists");

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -3,
                column: "Title",
                value: "Mua d? an v?t");

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -2,
                column: "Title",
                value: "Choi game 1 gi?");
        }
    }
}
