using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IlpRepoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSupabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    max_mark = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "batches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    batch_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    link = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    upload_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dus",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feedback_headers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    header = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_headers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mentors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mentor_type = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pocs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pocs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    technology = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<int>(type: "integer", nullable: true),
                    training_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hours = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_schedule", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_schedule_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "buddies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    du_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buddies", x => x.id);
                    table.ForeignKey(
                        name: "FK_buddies_dus_du_id",
                        column: x => x.du_id,
                        principalTable: "dus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "document_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: true),
                    document_id = table.Column<int>(type: "integer", nullable: true),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_requests_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_document_requests_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "menter_for_project",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mentor_id = table.Column<int>(type: "integer", nullable: false),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    mentor_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menter_for_project", x => x.id);
                    table.ForeignKey(
                        name: "FK_menter_for_project_mentors_mentor_id",
                        column: x => x.mentor_id,
                        principalTable: "mentors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menter_for_project_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pocs_for_project",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    poc_id = table.Column<int>(type: "integer", nullable: false),
                    project_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pocs_for_project", x => x.id);
                    table.ForeignKey(
                        name: "FK_pocs_for_project_pocs_poc_id",
                        column: x => x.poc_id,
                        principalTable: "pocs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pocs_for_project_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    link_id = table.Column<int>(type: "integer", nullable: true),
                    project_id = table.Column<int>(type: "integer", nullable: true),
                    link = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_links_links_link_id",
                        column: x => x.link_id,
                        principalTable: "links",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_project_links_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "trainees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    batch_id = table.Column<int>(type: "integer", nullable: false),
                    phone_no = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    blood_group = table.Column<string>(type: "text", nullable: true),
                    aadhaar_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    health_condition = table.Column<string>(type: "text", nullable: true),
                    personal_interest = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    emergency_contact_relationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    emergency_contact_no = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainees", x => x.id);
                    table.ForeignKey(
                        name: "FK_trainees_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainees_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<int>(type: "integer", nullable: true),
                    request_id = table.Column<int>(type: "integer", nullable: true),
                    file_name = table.Column<string>(type: "text", nullable: true),
                    file_type = table.Column<string>(type: "text", nullable: true),
                    submission_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_submissions_document_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "document_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_document_submissions_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    document_request_id = table.Column<int>(type: "integer", nullable: true),
                    message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_document_requests_document_request_id",
                        column: x => x.document_request_id,
                        principalTable: "document_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bo_phases",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainee_id = table.Column<int>(type: "integer", nullable: true),
                    buddy_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bo_phases", x => x.id);
                    table.ForeignKey(
                        name: "FK_bo_phases_buddies_buddy_id",
                        column: x => x.buddy_id,
                        principalTable: "buddies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bo_phases_trainees_trainee_id",
                        column: x => x.trainee_id,
                        principalTable: "trainees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainee_id = table.Column<int>(type: "integer", nullable: false),
                    assessment_type = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedbacks", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedbacks_trainees_trainee_id",
                        column: x => x.trainee_id,
                        principalTable: "trainees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_team",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    trainee_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_team", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_team_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_team_trainees_trainee_id",
                        column: x => x.trainee_id,
                        principalTable: "trainees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainee_id = table.Column<int>(type: "integer", nullable: true),
                    assessment_id = table.Column<int>(type: "integer", nullable: true),
                    obtained_mark = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_results_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_results_trainees_trainee_id",
                        column: x => x.trainee_id,
                        principalTable: "trainees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "trainee_activity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainee_id = table.Column<int>(type: "integer", nullable: true),
                    activity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    activity_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    details = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainee_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_trainee_activity_trainees_trainee_id",
                        column: x => x.trainee_id,
                        principalTable: "trainees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "trainee_du",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trainee_id = table.Column<int>(type: "integer", nullable: true),
                    du_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ojtMenter = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainee_du", x => x.id);
                    table.ForeignKey(
                        name: "FK_trainee_du_dus_du_id",
                        column: x => x.du_id,
                        principalTable: "dus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_trainee_du_trainees_trainee_id",
                        column: x => x.trainee_id,
                        principalTable: "trainees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "feedback_header_responses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    feedback_header_id = table.Column<int>(type: "integer", nullable: true),
                    feedback_id = table.Column<int>(type: "integer", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_header_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedback_header_responses_feedback_headers_feedback_header_~",
                        column: x => x.feedback_header_id,
                        principalTable: "feedback_headers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_feedback_header_responses_feedbacks_feedback_id",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bo_phases_buddy_id",
                table: "bo_phases",
                column: "buddy_id");

            migrationBuilder.CreateIndex(
                name: "IX_bo_phases_trainee_id",
                table: "bo_phases",
                column: "trainee_id");

            migrationBuilder.CreateIndex(
                name: "IX_buddies_du_id",
                table: "buddies",
                column: "du_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_requests_document_id",
                table: "document_requests",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_requests_project_id",
                table: "document_requests",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_submissions_document_id",
                table: "document_submissions",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_submissions_request_id",
                table: "document_submissions",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_header_responses_feedback_header_id",
                table: "feedback_header_responses",
                column: "feedback_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_header_responses_feedback_id",
                table: "feedback_header_responses",
                column: "feedback_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_trainee_id",
                table: "feedbacks",
                column: "trainee_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menter_for_project_mentor_id",
                table: "menter_for_project",
                column: "mentor_id");

            migrationBuilder.CreateIndex(
                name: "IX_menter_for_project_project_id",
                table: "menter_for_project",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentors_email",
                table: "mentors",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_document_request_id",
                table: "notifications",
                column: "document_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_pocs_email",
                table: "pocs",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pocs_for_project_poc_id",
                table: "pocs_for_project",
                column: "poc_id");

            migrationBuilder.CreateIndex(
                name: "IX_pocs_for_project_project_id",
                table: "pocs_for_project",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_links_link_id",
                table: "project_links",
                column: "link_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_links_project_id",
                table: "project_links",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_team_project_id_trainee_id",
                table: "project_team",
                columns: new[] { "project_id", "trainee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_team_trainee_id",
                table: "project_team",
                column: "trainee_id");

            migrationBuilder.CreateIndex(
                name: "IX_results_assessment_id",
                table: "results",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_results_trainee_id",
                table: "results",
                column: "trainee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainee_activity_trainee_id",
                table: "trainee_activity",
                column: "trainee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainee_du_du_id",
                table: "trainee_du",
                column: "du_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainee_du_trainee_id",
                table: "trainee_du",
                column: "trainee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainees_aadhaar_id",
                table: "trainees",
                column: "aadhaar_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainees_batch_id",
                table: "trainees",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_trainees_user_id",
                table: "trainees",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_schedule_batch_id",
                table: "training_schedule",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bo_phases");

            migrationBuilder.DropTable(
                name: "document_submissions");

            migrationBuilder.DropTable(
                name: "feedback_header_responses");

            migrationBuilder.DropTable(
                name: "menter_for_project");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "pocs_for_project");

            migrationBuilder.DropTable(
                name: "project_links");

            migrationBuilder.DropTable(
                name: "project_team");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "trainee_activity");

            migrationBuilder.DropTable(
                name: "trainee_du");

            migrationBuilder.DropTable(
                name: "training_schedule");

            migrationBuilder.DropTable(
                name: "buddies");

            migrationBuilder.DropTable(
                name: "feedback_headers");

            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "mentors");

            migrationBuilder.DropTable(
                name: "document_requests");

            migrationBuilder.DropTable(
                name: "pocs");

            migrationBuilder.DropTable(
                name: "links");

            migrationBuilder.DropTable(
                name: "assessments");

            migrationBuilder.DropTable(
                name: "dus");

            migrationBuilder.DropTable(
                name: "trainees");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "batches");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
