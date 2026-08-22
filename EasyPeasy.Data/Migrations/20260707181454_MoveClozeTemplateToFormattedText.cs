using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPeasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveClozeTemplateToFormattedText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing Cloze cards stored their {0},{1}-style template in `text` (Question).
            // The template now belongs in `formatted_text`, with `text` free for a short task instruction.
            migrationBuilder.Sql(@"
                UPDATE test_cards
                SET formatted_text = text,
                    text = NULL
                WHERE kind = 3 AND (formatted_text IS NULL OR formatted_text = '');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE test_cards
                SET text = formatted_text,
                    formatted_text = NULL
                WHERE kind = 3 AND (text IS NULL OR text = '');
            ");
        }
    }
}
