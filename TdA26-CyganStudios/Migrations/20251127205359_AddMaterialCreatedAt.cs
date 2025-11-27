using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdA26_CyganStudios.Migrations;

/// <inheritdoc />
public partial class AddMaterialCreatedAt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "CreatedAt",
            table: "UrlMaterials",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<long>(
            name: "CreatedAt",
            table: "FileMaterials",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0L);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "UrlMaterials");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "FileMaterials");
    }
}
