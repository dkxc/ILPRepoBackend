using IlpRepoBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IlpRepoBackend.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Trainee> Trainees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Mentor> Mentors { get; set; }
        public DbSet<Poc> Pocs { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<ProjectLink> ProjectLinks { get; set; }
        public DbSet<ProjectTeam> ProjectTeams { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<DocumentRequest> DocumentRequests { get; set; }
        public DbSet<DocumentSubmission> DocumentSubmissions { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<TrainingSchedule> TrainingSchedules { get; set; }
        public DbSet<TraineeActivity> TraineeActivities { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Du> Dus { get; set; }
        public DbSet<Buddy> Buddies { get; set; }
        public DbSet<BoPhase> BoPhases { get; set; }
        public DbSet<TraineeDu> TraineeDus { get; set; }
        public DbSet<FeedbackHeader> FeedbackHeaders { get; set; }
        public DbSet<FeedbackHeaderResponse> FeedbackHeaderResponses { get; set; }

        public DbSet<Phase> Phases { get; set; }
        public DbSet<PhaseType> PhaseTypes { get; set; }
        public DbSet<BatchType> BatchTypes { get; set; }
        public DbSet<PocsForProject> PocsForProjects { get; set; }
        public DbSet<MenterForAProject> MentersForProjects { get; set; }

        public DbSet<PocsForProject> PocsForProjects { get; set; }
        public DbSet<MenterForAProject> MentersForProjects { get; set; }


        public DbSet<EmailConfiguration> EmailConfigurations { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(startValue: 1)
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>().IsRequired();
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Trainee Configuration
            modelBuilder.Entity<Trainee>(entity =>
            {
                entity.ToTable("trainees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(startValue: 1)
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
                entity.Property(e => e.BatchId).HasColumnName("batch_id").IsRequired();
                entity.Property(e => e.PhoneNo).HasColumnName("phone_no").HasMaxLength(15);
                entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().IsRequired();
                entity.Property(e => e.BloodGroup).HasColumnName("blood_group").HasConversion<string>();
                entity.Property(e => e.AadhaarId).HasColumnName("aadhaar_id").HasMaxLength(20);
                entity.Property(e => e.HealthCondition).HasColumnName("health_condition").HasColumnType("text");
                entity.Property(e => e.PersonalInterest).HasColumnName("personal_interest").HasColumnType("text");
                entity.Property(e => e.Address).HasColumnName("address").HasColumnType("text");
                entity.Property(e => e.CurrentAddress).HasColumnName("current_address").HasColumnType("text");
                entity.Property(e => e.ContactNumber).HasColumnName("contact_number").HasMaxLength(15);
                entity.Property(e => e.EmergencyContactName).HasColumnName("emergency_contact_name").HasMaxLength(100);
                entity.Property(e => e.EmergencyContactRelationship).HasColumnName("emergency_contact_relationship").HasMaxLength(50);
                entity.Property(e => e.EmergencyContactNo).HasColumnName("emergency_contact_no").HasMaxLength(15);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.AadhaarId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Batch)
                    .WithMany(b => b.Trainees)
                    .HasForeignKey(e => e.BatchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BatchType configuration
            modelBuilder.Entity<BatchType>(entity =>
            {
                entity.ToTable("batch_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                // Seed default values with fixed DateTime values to avoid non-deterministic model
                entity.HasData(
                    new BatchType { Id = 1, Name = "Associate Software Developer", CreatedAt = new DateTime(2025, 10, 29, 12, 56, 6, DateTimeKind.Utc).AddTicks(9037), UpdatedAt = new DateTime(2025, 10, 29, 12, 56, 6, DateTimeKind.Utc).AddTicks(9335) },
                    new BatchType { Id = 2, Name = "SDET", CreatedAt = new DateTime(2025, 10, 29, 12, 56, 6, DateTimeKind.Utc).AddTicks(9578), UpdatedAt = new DateTime(2025, 10, 29, 12, 56, 6, DateTimeKind.Utc).AddTicks(9579) },
                    new BatchType { Id = 3, Name = "Business Analysis", CreatedAt = new DateTime(2025, 10, 29, 12, 56, 6, DateTimeKind.Utc).AddTicks(9580), UpdatedAt = new DateTime(2025, 10, 29, 12, 56, 6, DateTimeKind.Utc).AddTicks(9580) }
                );
            });

            // Batch Configuration
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.ToTable("batches");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.BatchName).HasColumnName("batch_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.BatchTypeId).HasColumnName("batch_type_id");
                entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().IsRequired();
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.BatchType)
                    .WithMany(bt => bt.Batches)
                    .HasForeignKey(e => e.BatchTypeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Project Configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.ProjectName).HasColumnName("project_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().IsRequired();
                entity.Property(e => e.Progress).HasColumnName("progress").HasDefaultValue(0);
                entity.Property(e => e.Technology).HasColumnName("technology").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            });

            // Mentor Configuration
            modelBuilder.Entity<Mentor>(entity =>
            {
                entity.ToTable("mentors");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(50);
                entity.Property(e => e.MentorType).HasColumnName("mentor_type")
                    .HasConversion<string>()
                    .IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(e => e.MenterForProjects)
                    .WithOne(mfp => mfp.Mentor)
                    .HasForeignKey(mfp => mfp.MenterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Poc Configuration
            modelBuilder.Entity<Poc>(entity =>
            {
                entity.ToTable("pocs");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(e => e.PocsForProjects)
                    .WithOne(pfp => pfp.Poc)
                    .HasForeignKey(pfp => pfp.PocId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Link Configuration
            modelBuilder.Entity<Link>(entity =>
            {
                entity.ToTable("links");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            });

            // ProjectLink Configuration
            modelBuilder.Entity<ProjectLink>(entity =>
            {
                entity.ToTable("project_links");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.LinkId).HasColumnName("link_id");
                entity.Property(e => e.ProjectId).HasColumnName("project_id");
                entity.Property(e => e.LinkUrl).HasColumnName("link").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Link)
                    .WithMany(l => l.ProjectLinks)
                    .HasForeignKey(e => e.LinkId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.ProjectLinks)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ProjectTeam Configuration - CHANGED: Added Id as primary key
            modelBuilder.Entity<ProjectTeam>(entity =>
            {
                entity.ToTable("project_team");
                entity.HasKey(e => e.Id); // Changed from composite key

                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.ProjectId).HasColumnName("project_id");
                entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
                entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>().IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                // Add unique constraint for the combination
                entity.HasIndex(e => new { e.ProjectId, e.TraineeId }).IsUnique();

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.ProjectTeams)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Trainee)
                    .WithMany(t => t.ProjectTeams)
                    .HasForeignKey(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Document Configuration
            modelBuilder.Entity<Documents>(entity =>
            {
                entity.ToTable("documents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Link).HasColumnName("link").HasMaxLength(255);
                entity.Property(e => e.FileType).HasColumnName("file_type").HasMaxLength(50);
                entity.Property(e => e.UploadDate).HasColumnName("upload_date").HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            });

            // DocumentRequest Configuration
            modelBuilder.Entity<DocumentRequest>(entity =>
            {
                entity.ToTable("document_requests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.ProjectId).HasColumnName("project_id");
                entity.Property(e => e.DocumentId).HasColumnName("document_id");
                entity.Property(e => e.RequestDate).HasColumnName("request_date").HasDefaultValueSql("now()");
                entity.Property(e => e.DueDate).HasColumnName("due_date").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.DocumentRequests)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.DocumentRequests)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // DocumentSubmission Configuration
            modelBuilder.Entity<DocumentSubmission>(entity =>
            {
                entity.ToTable("document_submissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.DocumentId).HasColumnName("document_id");
                entity.Property(e => e.FileName).HasColumnName("file_name");
                entity.Property(e => e.FileType).HasColumnName("file_type");
                entity.Property(e => e.RequestId).HasColumnName("request_id");
                entity.Property(e => e.SubmissionLink).HasColumnName("submission_link").HasMaxLength(500);
                entity.Property(e => e.SubmissionDate).HasColumnName("submission_date").HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.DocumentSubmissions)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.DocumentRequest)
                    .WithMany(dr => dr.DocumentSubmissions)
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Assessment Configuration
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.ToTable("assessments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>().IsRequired();
                entity.Property(e => e.MaxMark).HasColumnName("max_mark").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            });

            // Result Configuration
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("results");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
                entity.Property(e => e.AssessmentId).HasColumnName("assessment_id");
                entity.Property(e => e.ObtainedMark).HasColumnName("obtained_mark").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Trainee)
                    .WithMany(t => t.Results)
                    .HasForeignKey(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Assessment)
                    .WithMany(a => a.Results)
                    .HasForeignKey(e => e.AssessmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Feedback Configuration
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("feedbacks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.TraineeId).HasColumnName("trainee_id").IsRequired();
                entity.Property(e => e.AssessmentType).HasColumnName("assessment_type").HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasIndex(e => e.TraineeId).IsUnique();

                entity.HasOne(e => e.Trainee)
                    .WithOne(t => t.Feedback)
                    .HasForeignKey<Feedback>(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TrainingSchedule Configuration
            modelBuilder.Entity<TrainingSchedule>(entity =>
            {
                entity.ToTable("training_schedule");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.BatchId).HasColumnName("batch_id");
                entity.Property(e => e.TrainingDate).HasColumnName("training_date").IsRequired();
                entity.Property(e => e.Hours).HasColumnName("hours").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Batch)
                    .WithMany(b => b.TrainingSchedules)
                    .HasForeignKey(e => e.BatchId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TraineeActivity Configuration
            modelBuilder.Entity<TraineeActivity>(entity =>
            {
                entity.ToTable("trainee_activity");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
                entity.Property(e => e.ActivityType).HasColumnName("activity_type").HasMaxLength(50);
                entity.Property(e => e.ActivityTime).HasColumnName("activity_time").HasDefaultValueSql("now()");
                entity.Property(e => e.Details).HasColumnName("details").HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Trainee)
                    .WithMany(t => t.TraineeActivities)
                    .HasForeignKey(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Notification Configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.NotificationDate).HasColumnName("notification_date");
                entity.Property(e => e.DocumentRequestId).HasColumnName("document_request_id");
                entity.Property(e => e.Message).HasColumnName("message").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.DocumentRequest)
                    .WithMany(dr => dr.Notifications)
                    .HasForeignKey(e => e.DocumentRequestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Du Configuration
            modelBuilder.Entity<Du>(entity =>
            {
                entity.ToTable("dus");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            });

            // Buddy Configuration
            modelBuilder.Entity<Buddy>(entity =>
            {
                entity.ToTable("buddies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
                entity.Property(e => e.DuId).HasColumnName("du_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Du)
                    .WithMany(d => d.Buddies)
                    .HasForeignKey(e => e.DuId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // BoPhase Configuration
            modelBuilder.Entity<BoPhase>(entity =>
            {
                entity.ToTable("bo_phases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
                entity.Property(e => e.BuddyId).HasColumnName("buddy_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Trainee)
                    .WithMany(t => t.BoPhases)
                    .HasForeignKey(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Buddy)
                    .WithMany(b => b.BoPhases)
                    .HasForeignKey(e => e.BuddyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            // TraineeDu Configuration
            modelBuilder.Entity<TraineeDu>(entity =>
            {
                entity.ToTable("trainee_du");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
                entity.Property(e => e.DuId).HasColumnName("du_id");
                entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(255);
                entity.Property(e => e.ojtMenter).HasColumnName("ojtMenter").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.Trainee)
                    .WithMany(t => t.TraineeDus)
                    .HasForeignKey(e => e.TraineeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Du)
                    .WithMany(d => d.TraineeDus)
                    .HasForeignKey(e => e.DuId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // FeedbackHeader Configuration
            modelBuilder.Entity<FeedbackHeader>(entity =>
            {
                entity.ToTable("feedback_headers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Header).HasColumnName("header").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            });

            // FeedbackHeaderResponse Configuration
            modelBuilder.Entity<FeedbackHeaderResponse>(entity =>
            {
                entity.ToTable("feedback_header_responses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.FeedbackHeaderId).HasColumnName("feedback_header_id");
                entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
                entity.Property(e => e.Text).HasColumnName("text").HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.FeedbackHeader)
                    .WithMany(fh => fh.FeedbackHeaderResponses)
                    .HasForeignKey(e => e.FeedbackHeaderId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Feedback)
                    .WithMany(f => f.FeedbackHeaderResponses)
                    .HasForeignKey(e => e.FeedbackId)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            // Phase Configuration (moved from PhaseConfiguration)
            modelBuilder.Entity<Phase>(entity =>
            {
                entity.ToTable("phases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.PhaseType).HasColumnName("phase_type").IsRequired().HasMaxLength(50);
                entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnName("end_date").IsRequired();
                entity.Property(e => e.BatchId).HasColumnName("batch_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
                entity.Property(e => e.PhaseTypeId).HasColumnName("phase_type_id");

                entity.HasOne(e => e.Batch)
                    .WithMany(b => b.Phases)
                    .HasForeignKey(e => e.BatchId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PhaseTypeEntity)
                    .WithMany(pt => pt.Phases)
                    .HasForeignKey(e => e.PhaseTypeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PhaseType configuration
            modelBuilder.Entity<PhaseType>(entity =>
            {
                entity.ToTable("phase_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                // Seed default phase types (fixed DateTimes)
                entity.HasData(
                    new PhaseType { Id = 1, Name = "E Learning Phase", CreatedAt = new DateTime(2025, 10, 29, 13, 30, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 10, 29, 13, 30, 0, DateTimeKind.Utc) },
                    new PhaseType { Id = 2, Name = "Tech Fundamentals Phase", CreatedAt = new DateTime(2025, 10, 29, 13, 30, 1, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 10, 29, 13, 30, 1, DateTimeKind.Utc) },
                    new PhaseType { Id = 3, Name = "Specialization Phase", CreatedAt = new DateTime(2025, 10, 29, 13, 30, 2, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 10, 29, 13, 30, 2, DateTimeKind.Utc) },
                    new PhaseType { Id = 4, Name = "Business Orientation Phase", CreatedAt = new DateTime(2025, 10, 29, 13, 30, 3, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 10, 29, 13, 30, 3, DateTimeKind.Utc) },
                    new PhaseType { Id = 5, Name = "OJT Phase", CreatedAt = new DateTime(2025, 10, 29, 13, 30, 4, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 10, 29, 13, 30, 4, DateTimeKind.Utc) }
                );
                // PocsForProject Configuration - Already has Id
                modelBuilder.Entity<PocsForProject>(entity =>
                {
                    entity.ToTable("pocs_for_project");
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                    entity.Property(e => e.PocId).HasColumnName("poc_id").IsRequired();
                    entity.Property(e => e.ProjectId).HasColumnName("project_id").IsRequired();

                    entity.HasOne(e => e.Poc)
                        .WithMany(p => p.PocsForProjects)
                        .HasForeignKey(e => e.PocId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.Project)
                        .WithMany(p => p.PocsForProjects)
                        .HasForeignKey(e => e.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

                // MenterForAProject Configuration - Already has Id
                modelBuilder.Entity<MenterForAProject>(entity =>
                {
                    entity.ToTable("menter_for_project");
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                    entity.Property(e => e.MenterId).HasColumnName("mentor_id").IsRequired();
                    entity.Property(e => e.ProjectId).HasColumnName("project_id").IsRequired();
                    entity.Property(e => e.MentorType).HasColumnName("mentor_type").HasConversion<string>().IsRequired();

                    entity.HasOne(e => e.Project)
                        .WithMany(p => p.MentersForProjects)
                        .HasForeignKey(e => e.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(e => e.Mentor)
                        .WithMany(m => m.MenterForProjects)
                        .HasForeignKey(e => e.MenterId)
                        .OnDelete(DeleteBehavior.Cascade);
                });
                modelBuilder.Entity<EmailConfiguration>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.EmailSubject).IsRequired().HasMaxLength(500);
                    entity.Property(e => e.EmailBodyTemplate).IsRequired();
                });

                modelBuilder.Entity<EmailLog>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasOne(e => e.EmailConfiguration)
                        .WithMany()
                        .HasForeignKey(e => e.EmailConfigurationId)
                        .OnDelete(DeleteBehavior.Restrict);
                });
            });
        }
    }
}