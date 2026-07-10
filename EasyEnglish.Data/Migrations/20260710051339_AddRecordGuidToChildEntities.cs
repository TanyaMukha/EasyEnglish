using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordGuidToChildEntities : Migration
    {
        // SQLite не вміє підставити випадкове значення прямо через AddColumn(defaultValue: ...) —
        // тому кожен ADD COLUMN одразу супроводжується UPDATE, що генерує окремий v4-подібний
        // GUID для кожного вже наявного рядка (інакше всі старі рядки лишились би з однаковим
        // порожнім guid, що ламає звірку за GUID при імпорті ще до першого використання).
        private const string BackfillGuidSqlTemplate = """
            UPDATE {0} SET guid = (
                lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' ||
                substr(lower(hex(randomblob(2))),2) || '-' ||
                substr('89ab', abs(random()) % 4 + 1, 1) || substr(lower(hex(randomblob(2))),2) || '-' ||
                lower(hex(randomblob(6)))
            ) WHERE guid = '00000000-0000-0000-0000-000000000000';
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "guid",
                table: "words",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.Sql(string.Format(BackfillGuidSqlTemplate, "words"));

            migrationBuilder.AddColumn<Guid>(
                name: "guid",
                table: "test_cards",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.Sql(string.Format(BackfillGuidSqlTemplate, "test_cards"));

            migrationBuilder.AddColumn<Guid>(
                name: "guid",
                table: "study_cards",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.Sql(string.Format(BackfillGuidSqlTemplate, "study_cards"));

            migrationBuilder.AddColumn<Guid>(
                name: "guid",
                table: "irregular_forms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.Sql(string.Format(BackfillGuidSqlTemplate, "irregular_forms"));

            migrationBuilder.AddColumn<Guid>(
                name: "guid",
                table: "examples",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            migrationBuilder.Sql(string.Format(BackfillGuidSqlTemplate, "examples"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guid",
                table: "words");

            migrationBuilder.DropColumn(
                name: "guid",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "guid",
                table: "study_cards");

            migrationBuilder.DropColumn(
                name: "guid",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "guid",
                table: "examples");
        }
    }
}
