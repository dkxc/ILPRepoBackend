using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IlpRepoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPhaseTypeTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "phase_type_id",
                table: "phases",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "phase_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phase_types", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "phase_types",
                columns: new[] { "id", "created_at", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 29, 13, 30, 0, 0, DateTimeKind.Utc), "E Learning Phase", new DateTime(2025, 10, 29, 13, 30, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 10, 29, 13, 30, 1, 0, DateTimeKind.Utc), "Tech Fundamentals Phase", new DateTime(2025, 10, 29, 13, 30, 1, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 10, 29, 13, 30, 2, 0, DateTimeKind.Utc), "Specialization Phase", new DateTime(2025, 10, 29, 13, 30, 2, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 10, 29, 13, 30, 3, 0, DateTimeKind.Utc), "Business Orientation Phase", new DateTime(2025, 10, 29, 13, 30, 3, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 10, 29, 13, 30, 4, 0, DateTimeKind.Utc), "OJT Phase", new DateTime(2025, 10, 29, 13, 30, 4, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_phases_phase_type_id",
                table: "phases",
                column: "phase_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_phases_phase_types_phase_type_id",
                table: "phases",
                column: "phase_type_id",
                principalTable: "phase_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_phases_phase_types_phase_type_id",
                table: "phases");

            migrationBuilder.DropTable(
                name: "phase_types");

            migrationBuilder.DropIndex(
                name: "IX_phases_phase_type_id",
                table: "phases");

            migrationBuilder.DropColumn(
                name: "phase_type_id",
                table: "phases");
        }
    }
}
