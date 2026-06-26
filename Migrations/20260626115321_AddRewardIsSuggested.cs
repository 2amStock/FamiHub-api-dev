using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamiHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardIsSuggested : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuggested",
                table: "rewards",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -3,
                column: "IsSuggested",
                value: false);

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -2,
                column: "IsSuggested",
                value: false);

            migrationBuilder.UpdateData(
                table: "rewards",
                keyColumn: "Id",
                keyValue: -1,
                column: "IsSuggested",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuggested",
                table: "rewards");
        }
    }
}
