using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIrregularFormsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_words_irregular_word_forms_IrregularFormsFirstFormId_IrregularFormsSecondFormId_IrregularFormsThirdFormId",
                table: "words");

            migrationBuilder.DropTable(
                name: "irregular_word_forms");

            migrationBuilder.DropIndex(
                name: "IX_words_IrregularFormsFirstFormId_IrregularFormsSecondFormId_IrregularFormsThirdFormId",
                table: "words");

            migrationBuilder.DropColumn(
                name: "IrregularFormsFirstFormId",
                table: "words");

            migrationBuilder.DropColumn(
                name: "IrregularFormsSecondFormId",
                table: "words");

            migrationBuilder.DropColumn(
                name: "IrregularFormsThirdFormId",
                table: "words");

            migrationBuilder.RenameColumn(
                name: "word",
                table: "irregular_forms",
                newName: "second_form");

            migrationBuilder.RenameColumn(
                name: "translation",
                table: "irregular_forms",
                newName: "third_form_translation");

            migrationBuilder.RenameColumn(
                name: "transcription",
                table: "irregular_forms",
                newName: "third_form_transcription");

            migrationBuilder.RenameColumn(
                name: "pronunciation",
                table: "irregular_forms",
                newName: "third_form_pronunciation");

            migrationBuilder.AddColumn<string>(
                name: "first_form",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "first_form_pronunciation",
                table: "irregular_forms",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_form_transcription",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_form_translation",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "part_of_speech",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "second_form_pronunciation",
                table: "irregular_forms",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "second_form_transcription",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "second_form_translation",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "third_form",
                table: "irregular_forms",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_form",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "first_form_pronunciation",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "first_form_transcription",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "first_form_translation",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "part_of_speech",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "second_form_pronunciation",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "second_form_transcription",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "second_form_translation",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "third_form",
                table: "irregular_forms");

            migrationBuilder.RenameColumn(
                name: "third_form_translation",
                table: "irregular_forms",
                newName: "translation");

            migrationBuilder.RenameColumn(
                name: "third_form_transcription",
                table: "irregular_forms",
                newName: "transcription");

            migrationBuilder.RenameColumn(
                name: "third_form_pronunciation",
                table: "irregular_forms",
                newName: "pronunciation");

            migrationBuilder.RenameColumn(
                name: "second_form",
                table: "irregular_forms",
                newName: "word");

            migrationBuilder.AddColumn<int>(
                name: "IrregularFormsFirstFormId",
                table: "words",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IrregularFormsSecondFormId",
                table: "words",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IrregularFormsThirdFormId",
                table: "words",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "irregular_word_forms",
                columns: table => new
                {
                    first_form_id = table.Column<int>(type: "INTEGER", nullable: false),
                    second_form_id = table.Column<int>(type: "INTEGER", nullable: false),
                    third_form_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    last_review_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    review_count = table.Column<int>(type: "INTEGER", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_irregular_word_forms_words_first_form_id",
                        column: x => x.first_form_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_words_IrregularFormsFirstFormId_IrregularFormsSecondFormId_IrregularFormsThirdFormId",
                table: "words",
                columns: new[] { "IrregularFormsFirstFormId", "IrregularFormsSecondFormId", "IrregularFormsThirdFormId" });

            migrationBuilder.CreateIndex(
                name: "IX_irregular_word_forms_second_form_id",
                table: "irregular_word_forms",
                column: "second_form_id");

            migrationBuilder.CreateIndex(
                name: "IX_irregular_word_forms_third_form_id",
                table: "irregular_word_forms",
                column: "third_form_id");

            migrationBuilder.AddForeignKey(
                name: "FK_words_irregular_word_forms_IrregularFormsFirstFormId_IrregularFormsSecondFormId_IrregularFormsThirdFormId",
                table: "words",
                columns: new[] { "IrregularFormsFirstFormId", "IrregularFormsSecondFormId", "IrregularFormsThirdFormId" },
                principalTable: "irregular_word_forms",
                principalColumns: new[] { "first_form_id", "second_form_id", "third_form_id" });
        }
    }
}
