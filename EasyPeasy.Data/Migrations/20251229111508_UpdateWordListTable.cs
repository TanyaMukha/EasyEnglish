using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPeasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWordListTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "level",
                table: "word_lists",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "level",
                table: "word_lists");
        }
    }
}
