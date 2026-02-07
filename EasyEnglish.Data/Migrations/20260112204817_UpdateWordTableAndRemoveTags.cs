using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWordTableAndRemoveTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "word_tags");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropColumn(
                name: "definition",
                table: "words");

            migrationBuilder.DropColumn(
                name: "explanation",
                table: "words");

            migrationBuilder.DropColumn(
                name: "language",
                table: "words");

            migrationBuilder.DropColumn(
                name: "level",
                table: "words");

            migrationBuilder.DropColumn(
                name: "part_of_speech",
                table: "words");

            migrationBuilder.DropColumn(
                name: "level",
                table: "units");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "definition",
                table: "words",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "explanation",
                table: "words",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "words",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "level",
                table: "words",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "part_of_speech",
                table: "words",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "level",
                table: "units",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "word_tags",
                columns: table => new
                {
                    word_id = table.Column<int>(type: "INTEGER", nullable: false),
                    tag_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_word_tags", x => new { x.word_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_word_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_word_tags_words_word_id",
                        column: x => x.word_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_word_tags_tag_id",
                table: "word_tags",
                column: "tag_id");
        }
    }
}
