using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdA26_CyganStudios.Migrations;

/// <inheritdoc />
public partial class AddCourse : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "AspNetUserTokens",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "AspNetUsers",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT")
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<int>(
            name: "RoleId",
            table: "AspNetUserRoles",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "AspNetUserRoles",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "AspNetUserLogins",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "AspNetUserClaims",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "AspNetRoles",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT")
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<int>(
            name: "RoleId",
            table: "AspNetRoleClaims",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                LecturerId = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Uuid);
                table.ForeignKey(
                    name: "FK_Courses_AspNetUsers_LecturerId",
                    column: x => x.LecturerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Courses_LecturerId",
            table: "Courses",
            column: "LecturerId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Courses");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "AspNetUserTokens",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<string>(
            name: "Id",
            table: "AspNetUsers",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<string>(
            name: "RoleId",
            table: "AspNetUserRoles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "AspNetUserRoles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "AspNetUserLogins",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "AspNetUserClaims",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<string>(
            name: "Id",
            table: "AspNetRoles",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<string>(
            name: "RoleId",
            table: "AspNetRoleClaims",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER");
    }
}
