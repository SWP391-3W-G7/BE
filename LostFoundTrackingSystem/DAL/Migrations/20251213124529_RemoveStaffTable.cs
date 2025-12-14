using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaffTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__ReturnRec__Staff__1DB06A4F",
                table: "ReturnRecord");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.RenameColumn(
                name: "StaffID",
                table: "ReturnRecord",
                newName: "StaffUserID");

            // migrationBuilder.RenameIndex(
            //     name: "IX_ReturnRecord_StaffID",
            //     table: "ReturnRecord",
            //     newName: "IX_ReturnRecord_StaffUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnRecord_User_Staff",
                table: "ReturnRecord",
                column: "StaffUserID",
                principalTable: "User",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnRecord_User_Staff",
                table: "ReturnRecord");

            migrationBuilder.RenameColumn(
                name: "StaffUserID",
                table: "ReturnRecord",
                newName: "StaffID");

            // migrationBuilder.RenameIndex(
            //     name: "IX_ReturnRecord_StaffUserID",
            //     table: "ReturnRecord",
            //     newName: "IX_ReturnRecord_StaffID");

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Staff__96D4AAF719481B07", x => x.StaffID);
                    table.ForeignKey(
                        name: "FK__Staff__CampusID__0C85DE4D",
                        column: x => x.CampusID,
                        principalTable: "Campus",
                        principalColumn: "CampusID");
                    table.ForeignKey(
                        name: "FK__Staff__UserID__0B91BA14",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_CampusID",
                table: "Staff",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "UQ__Staff__1788CCADBD1D05C3",
                table: "Staff",
                column: "UserID",
                unique: true,
                filter: "[UserID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK__ReturnRec__Staff__1DB06A4F",
                table: "ReturnRecord",
                column: "StaffID",
                principalTable: "Staff",
                principalColumn: "StaffID");
        }
    }
}
