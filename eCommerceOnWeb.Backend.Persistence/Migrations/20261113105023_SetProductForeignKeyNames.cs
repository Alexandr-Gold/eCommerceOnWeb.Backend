using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceOnWeb.Backend.Persistence.Migrations;

/// <inheritdoc />
public partial class SetProductForeignKeyNames : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_products_gtin",
            table: "products");

        migrationBuilder.CreateIndex(
            name: "IX_products_gtin",
            table: "products",
            column: "gtin",
            unique: true,
            filter: "gtin IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_products_gtin",
            table: "products");

        migrationBuilder.CreateIndex(
            name: "IX_products_gtin",
            table: "products",
            column: "gtin",
            unique: true);
    }
}
