using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class DictionaryToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_cards_units_unit_id",
                table: "study_cards");

            migrationBuilder.DropForeignKey(
                name: "FK_word_lists_dictionaries_dictionary_id",
                table: "word_lists");

            migrationBuilder.DropTable(
                name: "dictionaries");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropIndex(
                name: "IX_study_cards_unit_id",
                table: "study_cards");

            migrationBuilder.DropColumn(
                name: "unit_id",
                table: "study_cards");

            migrationBuilder.RenameColumn(
                name: "dictionary_id",
                table: "word_lists",
                newName: "course_id");

            migrationBuilder.RenameIndex(
                name: "IX_word_lists_dictionary_id",
                table: "word_lists",
                newName: "IX_word_lists_course_id");

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_word_lists_courses_course_id",
                table: "word_lists",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_word_lists_courses_course_id",
                table: "word_lists");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "word_lists",
                newName: "dictionary_id");

            migrationBuilder.RenameIndex(
                name: "IX_word_lists_course_id",
                table: "word_lists",
                newName: "IX_word_lists_dictionary_id");

            migrationBuilder.AddColumn<int>(
                name: "unit_id",
                table: "study_cards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "dictionaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dictionaries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_cards_unit_id",
                table: "study_cards",
                column: "unit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_study_cards_units_unit_id",
                table: "study_cards",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_word_lists_dictionaries_dictionary_id",
                table: "word_lists",
                column: "dictionary_id",
                principalTable: "dictionaries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
