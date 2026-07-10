using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyEnglish.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "subject_id",
                table: "courses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_courses_subject_id",
                table: "courses",
                column: "subject_id");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_subjects_subject_id",
                table: "courses",
                column: "subject_id",
                principalTable: "subjects",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_subjects_subject_id",
                table: "courses");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_courses_subject_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "subject_id",
                table: "courses");
        }
    }
}
