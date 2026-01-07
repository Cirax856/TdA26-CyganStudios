using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdA26_CyganStudios.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmitedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SubmitedAt",
                table: "QuizSubmisions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmitedAt",
                table: "QuizSubmisions");
        }
    }
}
