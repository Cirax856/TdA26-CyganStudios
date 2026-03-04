using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdA26_CyganStudios.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledPublish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ScheduledPublishAt",
                table: "Courses",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledPublishAt",
                table: "Courses");
        }
    }
}
