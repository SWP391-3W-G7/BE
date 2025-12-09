using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLostItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "User",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CampusID",
                table: "LostItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LostItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LostItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LostItemID",
                table: "Image",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LostItem_CampusID",
                table: "LostItem",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_Image_LostItemID",
                table: "Image",
                column: "LostItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_LostItem",
                table: "Image",
                column: "LostItemID",
                principalTable: "LostItem",
                principalColumn: "LostItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_LostItem_Campus",
                table: "LostItem",
                column: "CampusID",
                principalTable: "Campus",
                principalColumn: "CampusID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_LostItem",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_LostItem_Campus",
                table: "LostItem");

            migrationBuilder.DropIndex(
                name: "IX_LostItem_CampusID",
                table: "LostItem");

            migrationBuilder.DropIndex(
                name: "IX_Image_LostItemID",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CampusID",
                table: "LostItem");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LostItem");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "LostItem");

            migrationBuilder.DropColumn(
                name: "LostItemID",
                table: "Image");
        }
    }
}
