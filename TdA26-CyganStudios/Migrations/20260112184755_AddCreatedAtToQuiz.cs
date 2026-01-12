using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdA26_CyganStudios.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "Quizzes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Quizzes");
        }
    }
}
