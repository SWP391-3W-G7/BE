using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLostItemsAndReturnRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LostItemID",
                table: "ReturnRecord",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReturnRecord_LostItemId",
                table: "ReturnRecord",
                column: "LostItemID",
                unique: true,
                filter: "[LostItemID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnRecord_LostItem",
                table: "ReturnRecord",
                column: "LostItemID",
                principalTable: "LostItem",
                principalColumn: "LostItemID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnRecord_LostItem",
                table: "ReturnRecord");

            migrationBuilder.DropIndex(
                name: "UQ_ReturnRecord_LostItemId",
                table: "ReturnRecord");

            migrationBuilder.DropColumn(
                name: "LostItemID",
                table: "ReturnRecord");
        }
    }
}
