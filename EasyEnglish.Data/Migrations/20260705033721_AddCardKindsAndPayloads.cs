using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCardKindsAndPayloads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "mask",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "test_type",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "description",
                table: "study_cards");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "test_cards",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "text",
                table: "test_cards",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "kind",
                table: "test_cards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "study_cards",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code_block",
                table: "study_cards",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "kind",
                table: "study_cards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "reveal_mode",
                table: "study_cards",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kind",
                table: "test_cards");

            migrationBuilder.DropColumn(
                name: "body",
                table: "study_cards");

            migrationBuilder.DropColumn(
                name: "code_block",
                table: "study_cards");

            migrationBuilder.DropColumn(
                name: "kind",
                table: "study_cards");

            migrationBuilder.DropColumn(
                name: "reveal_mode",
                table: "study_cards");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "test_cards",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "text",
                table: "test_cards",
                type: "TEXT",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "test_cards",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mask",
                table: "test_cards",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "test_type",
                table: "test_cards",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "study_cards",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
