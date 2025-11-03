using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IlpRepoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedBatchTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "batch_type",
                table: "batches");

            migrationBuilder.AddColumn<int>(
                name: "batch_type_id",
                table: "batches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "batch_types",
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
                    table.PrimaryKey("PK_batch_types", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "batch_types",
                columns: new[] { "id", "created_at", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9037), "Associate Software Developer", new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9335) },
                    { 2, new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9578), "SDET", new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9579) },
                    { 3, new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9580), "Business Analysis", new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9580) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_batches_batch_type_id",
                table: "batches",
                column: "batch_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_batches_batch_types_batch_type_id",
                table: "batches",
                column: "batch_type_id",
                principalTable: "batch_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_batches_batch_types_batch_type_id",
                table: "batches");

            migrationBuilder.DropTable(
                name: "batch_types");

            migrationBuilder.DropIndex(
                name: "IX_batches_batch_type_id",
                table: "batches");

            migrationBuilder.DropColumn(
                name: "batch_type_id",
                table: "batches");

            migrationBuilder.AddColumn<string>(
                name: "batch_type",
                table: "batches",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
