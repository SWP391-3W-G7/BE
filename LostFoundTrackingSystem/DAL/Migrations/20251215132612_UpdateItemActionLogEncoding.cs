using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemActionLogEncoding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OldStatus",
                table: "ItemActionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "Vietnamese_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewStatus",
                table: "ItemActionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "Vietnamese_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "ItemActionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "Vietnamese_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionDetails",
                table: "ItemActionLog",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Vietnamese_CI_AS",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OldStatus",
                table: "ItemActionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldCollation: "Vietnamese_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "NewStatus",
                table: "ItemActionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldCollation: "Vietnamese_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "ItemActionLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldCollation: "Vietnamese_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "ActionDetails",
                table: "ItemActionLog",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Vietnamese_CI_AS");
        }
    }
}
