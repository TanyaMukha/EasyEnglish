using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPeasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class WordListToUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_words_word_lists_word_list_id",
                table: "words");

            migrationBuilder.DropTable(
                name: "word_lists");

            migrationBuilder.RenameColumn(
                name: "word_list_id",
                table: "words",
                newName: "unit_id");

            migrationBuilder.RenameIndex(
                name: "IX_words_word_list_id",
                table: "words",
                newName: "IX_words_unit_id");

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    level = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    course_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.id);
                    table.ForeignKey(
                        name: "FK_units_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_units_course_id",
                table: "units",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_words_units_unit_id",
                table: "words",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_words_units_unit_id",
                table: "words");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.RenameColumn(
                name: "unit_id",
                table: "words",
                newName: "word_list_id");

            migrationBuilder.RenameIndex(
                name: "IX_words_unit_id",
                table: "words",
                newName: "IX_words_word_list_id");

            migrationBuilder.CreateTable(
                name: "word_lists",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    course_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    level = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_word_lists", x => x.id);
                    table.ForeignKey(
                        name: "FK_word_lists_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_word_lists_course_id",
                table: "word_lists",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_words_word_lists_word_list_id",
                table: "words",
                column: "word_list_id",
                principalTable: "word_lists",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
