using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IlpRepoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddednewCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "link",
                table: "documents");

            migrationBuilder.AddColumn<string>(
                name: "template_link",
                table: "documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "document_submissions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "file_type",
                table: "document_submissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "project_id",
                table: "document_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "trainee_id",
                table: "document_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "file_url",
                table: "document_requests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_submissions_project_id",
                table: "document_submissions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_submissions_trainee_id",
                table: "document_submissions",
                column: "trainee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_submissions_projects_project_id",
                table: "document_submissions",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_document_submissions_trainees_trainee_id",
                table: "document_submissions",
                column: "trainee_id",
                principalTable: "trainees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_projects_project_id",
                table: "document_submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_document_submissions_trainees_trainee_id",
                table: "document_submissions");

            migrationBuilder.DropIndex(
                name: "IX_document_submissions_project_id",
                table: "document_submissions");

            migrationBuilder.DropIndex(
                name: "IX_document_submissions_trainee_id",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "template_link",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "type",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "file_name",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "file_type",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "trainee_id",
                table: "document_submissions");

            migrationBuilder.DropColumn(
                name: "file_url",
                table: "document_requests");

            migrationBuilder.AddColumn<string>(
                name: "link",
                table: "documents",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
