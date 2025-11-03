using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IlpRepoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedBatchTypeTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "batch_types",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 29, 12, 56, 6, 0, DateTimeKind.Utc).AddTicks(9037), new DateTime(2025, 10, 29, 12, 56, 6, 0, DateTimeKind.Utc).AddTicks(9335) });

            migrationBuilder.UpdateData(
                table: "batch_types",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 29, 12, 56, 6, 0, DateTimeKind.Utc).AddTicks(9578), new DateTime(2025, 10, 29, 12, 56, 6, 0, DateTimeKind.Utc).AddTicks(9579) });

            migrationBuilder.UpdateData(
                table: "batch_types",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 29, 12, 56, 6, 0, DateTimeKind.Utc).AddTicks(9580), new DateTime(2025, 10, 29, 12, 56, 6, 0, DateTimeKind.Utc).AddTicks(9580) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "batch_types",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9037), new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9335) });

            migrationBuilder.UpdateData(
                table: "batch_types",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9578), new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9579) });

            migrationBuilder.UpdateData(
                table: "batch_types",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9580), new DateTime(2025, 10, 29, 12, 56, 6, 234, DateTimeKind.Utc).AddTicks(9580) });
        }
    }
}
