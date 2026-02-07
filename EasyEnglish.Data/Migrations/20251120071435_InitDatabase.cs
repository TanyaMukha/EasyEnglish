using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dictionaries",
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
                    table.PrimaryKey("PK_dictionaries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grammar_tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    parent_topic_id = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "irregular_forms",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    word = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    transcription = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    translation = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    pronunciation = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_irregular_forms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "word_lists",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    dictionary_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_word_lists", x => x.id);
                    table.ForeignKey(
                        name: "FK_word_lists_dictionaries_dictionary_id",
                        column: x => x.dictionary_id,
                        principalTable: "dictionaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_cards",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    test_type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    mask = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    options = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    correct_answers = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    rate = table.Column<float>(type: "REAL", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    test_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_cards_grammar_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "grammar_tests",
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

            migrationBuilder.CreateTable(
                name: "study_cards",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    dialogue = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    rate = table.Column<float>(type: "REAL", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    unit_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_cards_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examples",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sentence = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    translation = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    word_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examples", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "irregular_word_forms",
                columns: table => new
                {
                    first_form_id = table.Column<int>(type: "INTEGER", nullable: false),
                    second_form_id = table.Column<int>(type: "INTEGER", nullable: false),
                    third_form_id = table.Column<int>(type: "INTEGER", nullable: false),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_irregular_word_forms", x => new { x.first_form_id, x.second_form_id, x.third_form_id });
                    table.ForeignKey(
                        name: "FK_irregular_word_forms_irregular_forms_second_form_id",
                        column: x => x.second_form_id,
                        principalTable: "irregular_forms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_irregular_word_forms_irregular_forms_third_form_id",
                        column: x => x.third_form_id,
                        principalTable: "irregular_forms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "words",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    word = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    transcription = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    translation = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    explanation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    definition = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    part_of_speech = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    level = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    pronunciation = table.Column<byte[]>(type: "BLOB", nullable: true),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    rate = table.Column<float>(type: "REAL", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    word_list_id = table.Column<int>(type: "INTEGER", nullable: false),
                    IrregularFormsFirstFormId = table.Column<int>(type: "INTEGER", nullable: true),
                    IrregularFormsSecondFormId = table.Column<int>(type: "INTEGER", nullable: true),
                    IrregularFormsThirdFormId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_words", x => x.id);
                    table.ForeignKey(
                        name: "FK_words_irregular_word_forms_IrregularFormsFirstFormId_IrregularFormsSecondFormId_IrregularFormsThirdFormId",
                        columns: x => new { x.IrregularFormsFirstFormId, x.IrregularFormsSecondFormId, x.IrregularFormsThirdFormId },
                        principalTable: "irregular_word_forms",
                        principalColumns: new[] { "first_form_id", "second_form_id", "third_form_id" });
                    table.ForeignKey(
                        name: "FK_words_word_lists_word_list_id",
                        column: x => x.word_list_id,
                        principalTable: "word_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_examples_word_id",
                table: "examples",
                column: "word_id");

            migrationBuilder.CreateIndex(
                name: "IX_grammar_topic_tests_test_id",
                table: "grammar_topic_tests",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_grammar_topics_parent_topic_id",
                table: "grammar_topics",
                column: "parent_topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_irregular_word_forms_second_form_id",
                table: "irregular_word_forms",
                column: "second_form_id");

            migrationBuilder.CreateIndex(
                name: "IX_irregular_word_forms_third_form_id",
                table: "irregular_word_forms",
                column: "third_form_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_card_items_word_id",
                table: "study_card_items",
                column: "word_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_cards_unit_id",
                table: "study_cards",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_cards_test_id",
                table: "test_cards",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_word_lists_dictionary_id",
                table: "word_lists",
                column: "dictionary_id");

            migrationBuilder.CreateIndex(
                name: "IX_word_tags_tag_id",
                table: "word_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_words_IrregularFormsFirstFormId_IrregularFormsSecondFormId_IrregularFormsThirdFormId",
                table: "words",
                columns: new[] { "IrregularFormsFirstFormId", "IrregularFormsSecondFormId", "IrregularFormsThirdFormId" });

            migrationBuilder.CreateIndex(
                name: "IX_words_word_list_id",
                table: "words",
                column: "word_list_id");

            migrationBuilder.AddForeignKey(
                name: "FK_examples_words_word_id",
                table: "examples",
                column: "word_id",
                principalTable: "words",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_irregular_word_forms_words_first_form_id",
                table: "irregular_word_forms",
                column: "first_form_id",
                principalTable: "words",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_irregular_word_forms_words_first_form_id",
                table: "irregular_word_forms");

            migrationBuilder.DropTable(
                name: "examples");

            migrationBuilder.DropTable(
                name: "grammar_topic_tests");

            migrationBuilder.DropTable(
                name: "study_card_items");

            migrationBuilder.DropTable(
                name: "test_cards");

            migrationBuilder.DropTable(
                name: "word_tags");

            migrationBuilder.DropTable(
                name: "grammar_topics");

            migrationBuilder.DropTable(
                name: "study_cards");

            migrationBuilder.DropTable(
                name: "grammar_tests");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "words");

            migrationBuilder.DropTable(
                name: "irregular_word_forms");

            migrationBuilder.DropTable(
                name: "word_lists");

            migrationBuilder.DropTable(
                name: "irregular_forms");

            migrationBuilder.DropTable(
                name: "dictionaries");
        }
    }
}
