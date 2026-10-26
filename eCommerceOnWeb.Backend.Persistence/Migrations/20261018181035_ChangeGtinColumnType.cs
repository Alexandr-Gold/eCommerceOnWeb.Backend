using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceOnWeb.Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGtinColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "gtin",
                table: "products",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character(13)",
                oldFixedLength: true,
                oldMaxLength: 13,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "gtin",
                table: "products",
                type: "character(13)",
                fixedLength: true,
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);
        }
    }
}
