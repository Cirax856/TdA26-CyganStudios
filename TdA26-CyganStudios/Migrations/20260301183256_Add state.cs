using Microsoft.EntityFrameworkCore.Migrations;
using TdA26_CyganStudios.Models.Db;

#nullable disable

namespace TdA26_CyganStudios.Migrations;

/// <inheritdoc />
public partial class Addstate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "State",
            table: "Courses",
            type: "INTEGER",
            nullable: false,
            defaultValue: (int)CourseState.Published);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "State",
            table: "Courses");
    }
}
