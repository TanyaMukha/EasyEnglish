using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPeasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnitsTableRemoveGrammarTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_test_cards_grammar_tests_test_id",
                table: "test_cards");

            migrationBuilder.DropTable(
                name: "grammar_topic_tests");

            migrationBuilder.DropTable(
                name: "study_card_items");

            migrationBuilder.DropTable(
                name: "grammar_tests");

            migrationBuilder.DropTable(
                name: "grammar_topics");

            migrationBuilder.RenameColumn(
                name: "test_id",
                table: "test_cards",
                newName: "unit_id");

            migrationBuilder.RenameIndex(
                name: "IX_test_cards_test_id",
                table: "test_cards",
                newName: "IX_test_cards_unit_id");

            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "units",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "unit_id",
                table: "study_cards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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
                name: "FK_test_cards_units_unit_id",
                table: "test_cards",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_cards_units_unit_id",
                table: "study_cards");

            migrationBuilder.DropForeignKey(
                name: "FK_test_cards_units_unit_id",
                table: "test_cards");

            migrationBuilder.DropIndex(
                name: "IX_study_cards_unit_id",
                table: "study_cards");

            migrationBuilder.DropColumn(
                name: "content",
                table: "units");

            migrationBuilder.DropColumn(
                name: "unit_id",
                table: "study_cards");

            migrationBuilder.RenameColumn(
                name: "unit_id",
                table: "test_cards",
                newName: "test_id");

            migrationBuilder.RenameIndex(
                name: "IX_test_cards_unit_id",
                table: "test_cards",
                newName: "IX_test_cards_test_id");

            migrationBuilder.CreateTable(
                name: "grammar_tests",
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
                    table.PrimaryKey("PK_grammar_tests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grammar_topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    parent_topic_id = table.Column<int>(type: "INTEGER", nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grammar_topics", x => x.id);
                    table.ForeignKey(
                        name: "FK_grammar_topics_grammar_topics_parent_topic_id",
                        column: x => x.parent_topic_id,
                        principalTable: "grammar_topics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "study_card_items",
                columns: table => new
                {
                    card_id = table.Column<int>(type: "INTEGER", nullable: false),
                    word_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_card_items", x => new { x.card_id, x.word_id });
                    table.ForeignKey(
                        name: "FK_study_card_items_study_cards_card_id",
                        column: x => x.card_id,
                        principalTable: "study_cards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_study_card_items_words_word_id",
                        column: x => x.word_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grammar_topic_tests",
                columns: table => new
                {
                    topic_id = table.Column<int>(type: "INTEGER", nullable: false),
                    test_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grammar_topic_tests", x => new { x.topic_id, x.test_id });
                    table.ForeignKey(
                        name: "FK_grammar_topic_tests_grammar_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "grammar_tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grammar_topic_tests_grammar_topics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "grammar_topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grammar_topic_tests_test_id",
                table: "grammar_topic_tests",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_grammar_topics_parent_topic_id",
                table: "grammar_topics",
                column: "parent_topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_card_items_word_id",
                table: "study_card_items",
                column: "word_id");

            migrationBuilder.AddForeignKey(
                name: "FK_test_cards_grammar_tests_test_id",
                table: "test_cards",
                column: "test_id",
                principalTable: "grammar_tests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
