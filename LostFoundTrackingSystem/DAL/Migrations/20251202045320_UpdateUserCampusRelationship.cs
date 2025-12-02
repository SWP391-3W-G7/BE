using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserCampusRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campus",
                columns: table => new
                {
                    CampusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusName = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Campus__FD598D36729196F6", x => x.CampusID);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__8AFACE3A2D5E534F", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CCAC7ADB6FA1", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_User_Role",
                        column: x => x.RoleID,
                        principalTable: "Role",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "FoundItem",
                columns: table => new
                {
                    FoundItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FoundDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    FoundLocation = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    StoredBy = table.Column<int>(type: "int", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoundIte__DFA62C370CAF2C28", x => x.FoundItemID);
                    table.ForeignKey(
                        name: "FK_Found_Campus",
                        column: x => x.CampusID,
                        principalTable: "Campus",
                        principalColumn: "CampusID");
                    table.ForeignKey(
                        name: "FK_Found_User",
                        column: x => x.StoredBy,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "LostItem",
                columns: table => new
                {
                    LostItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LostDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Location = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LostItem__3B3BF432EB9C7F4F", x => x.LostItemID);
                    table.ForeignKey(
                        name: "FK_Lost_Campus",
                        column: x => x.CampusID,
                        principalTable: "Campus",
                        principalColumn: "CampusID");
                    table.ForeignKey(
                        name: "FK_Lost_User",
                        column: x => x.CreatedBy,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Claim",
                columns: table => new
                {
                    ClaimID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Evidence = table.Column<string>(type: "text", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    FoundItemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Claim__EF2E13BB6FD5146B", x => x.ClaimID);
                    table.ForeignKey(
                        name: "FK_Claim_Found",
                        column: x => x.FoundItemID,
                        principalTable: "FoundItem",
                        principalColumn: "FoundItemID");
                    table.ForeignKey(
                        name: "FK_Claim_User",
                        column: x => x.StudentID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "ReturnRecord",
                columns: table => new
                {
                    ReturnID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReceiverID = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    FoundItemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReturnRe__F445E988E2C6B73A", x => x.ReturnID);
                    table.ForeignKey(
                        name: "FK_Return_Found",
                        column: x => x.FoundItemID,
                        principalTable: "FoundItem",
                        principalColumn: "FoundItemID");
                    table.ForeignKey(
                        name: "FK_Return_Receiver",
                        column: x => x.ReceiverID,
                        principalTable: "User",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Return_Staff",
                        column: x => x.StaffID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claim_FoundItemID",
                table: "Claim",
                column: "FoundItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Claim_StudentID",
                table: "Claim",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_FoundItem_CampusID",
                table: "FoundItem",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_FoundItem_StoredBy",
                table: "FoundItem",
                column: "StoredBy");

            migrationBuilder.CreateIndex(
                name: "IX_LostItem_CampusID",
                table: "LostItem",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_LostItem_CreatedBy",
                table: "LostItem",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRecord_ReceiverID",
                table: "ReturnRecord",
                column: "ReceiverID");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRecord_StaffID",
                table: "ReturnRecord",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "UQ__ReturnRe__DFA62C368DFE4873",
                table: "ReturnRecord",
                column: "FoundItemID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleID",
                table: "User",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D105347BDC93EF",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "LostItem");

            migrationBuilder.DropTable(
                name: "ReturnRecord");

            migrationBuilder.DropTable(
                name: "FoundItem");

            migrationBuilder.DropTable(
                name: "Campus");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
