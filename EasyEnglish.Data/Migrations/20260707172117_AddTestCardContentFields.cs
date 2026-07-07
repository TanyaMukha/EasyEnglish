using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCardContentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "explanation",
                table: "test_cards",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "formatted_text",
                table: "test_cards",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "image",
                table: "test_cards",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "explanation",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "formatted_text",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "image",
                table: "test_cards");
        }
    }
}
