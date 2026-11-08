using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerceOnWeb.Backend.Persistence.Migrations;

/// <inheritdoc />
public partial class AddProductStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_active",
            table: "products");

        migrationBuilder.AddColumn<string>(
            name: "status",
            table: "products",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Draft");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "status",
            table: "products");

        migrationBuilder.AddColumn<bool>(
            name: "is_active",
            table: "products",
            type: "boolean",
            nullable: false,
            defaultValue: true);
    }
}
