using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPeasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseLanguageAndWordNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "words",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "courses",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note",
                table: "words");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "courses");
        }
    }
}
