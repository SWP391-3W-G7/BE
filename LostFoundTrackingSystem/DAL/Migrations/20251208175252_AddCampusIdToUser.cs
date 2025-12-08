using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCampusIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_CampusId",
                table: "User",
                column: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Campus",
                table: "User",
                column: "CampusId",
                principalTable: "Campus",
                principalColumn: "CampusID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Campus",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_CampusId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "User");
        }
    }
}
