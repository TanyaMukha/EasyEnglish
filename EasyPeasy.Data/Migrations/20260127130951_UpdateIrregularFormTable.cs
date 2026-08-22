using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPeasy.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIrregularFormTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "irregular_forms",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "last_review_date",
                table: "irregular_forms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "rate",
                table: "irregular_forms",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "review_count",
                table: "irregular_forms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "unit_id",
                table: "irregular_forms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "irregular_forms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_irregular_forms_unit_id",
                table: "irregular_forms",
                column: "unit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_irregular_forms_units_unit_id",
                table: "irregular_forms",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_irregular_forms_units_unit_id",
                table: "irregular_forms");

            migrationBuilder.DropIndex(
                name: "IX_irregular_forms_unit_id",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "last_review_date",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "rate",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "review_count",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "unit_id",
                table: "irregular_forms");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "irregular_forms");
        }
    }
}
