using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdA26_CyganStudios.Migrations;

/// <inheritdoc />
public partial class AddMaterials : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FileMaterials",
            columns: table => new
            {
                Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: false),
                FileUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                MimeType = table.Column<string>(type: "TEXT", nullable: false),
                SizeInBytes = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FileMaterials", x => x.Uuid);
                table.ForeignKey(
                    name: "FK_FileMaterials_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Uuid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UrlMaterials",
            columns: table => new
            {
                Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                CourseId = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: false),
                Url = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UrlMaterials", x => x.Uuid);
                table.ForeignKey(
                    name: "FK_UrlMaterials_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Uuid",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FileMaterials_CourseId",
            table: "FileMaterials",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_UrlMaterials_CourseId",
            table: "UrlMaterials",
            column: "CourseId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "FileMaterials");

        migrationBuilder.DropTable(
            name: "UrlMaterials");
    }
}
