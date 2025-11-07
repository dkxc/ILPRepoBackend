using IlpRepoBackend.Application.Dto.Dashboard;
using IlpRepoBackend.Application.Handler.Dashboard;
using IlpRepoBackend.Application.Query.Dashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;
using CurriculumEntity = IlpRepoBackend.Domain.Entities.Curriculum;

namespace IlpRepoBackend.Test.Handlers.Dashboard
{
    public class GetTraineeDashboardQueryHandlerTests
    {
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IDocumentRequestRepository> _documentRequestRepositoryMock;
        private readonly GetTraineeDashboardQueryHandler _handler;

        public GetTraineeDashboardQueryHandlerTests()
        {
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _documentRequestRepositoryMock = new Mock<IDocumentRequestRepository>();
            
            _handler = new GetTraineeDashboardQueryHandler(
                _traineeRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _curriculumRepositoryMock.Object,
                _documentRepositoryMock.Object,
                _documentRequestRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_ReturnsCompleteDashboard()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;
            var projectId = 1;

            var user = new User
            {
                Id = userId,
                Username = "John Doe",
                Email = "john@example.com"
            };

            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch 2024",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var project = new Project
            {
                Id = projectId,
                ProjectName = "E-Commerce App",
                Status = ProjectStatus.Live,
                Progress = 50,
                Technology = "React, Node.js",
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam { TraineeId = 1, Trainee = new Trainee { User = new User { Username = "John Doe" } } },
                    new ProjectTeam { TraineeId = 2, Trainee = new Trainee { User = new User { Username = "Jane Smith" } } }
                }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                Email = "john@example.com",
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam { ProjectId = projectId, TraineeId = 1 }
                },
                Results = new List<Result>()
            };

            var allTrainees = new List<Trainee> { trainee };
            var curriculums = new List<CurriculumEntity>
            {
                new CurriculumEntity { Id = 1, Title = "Introduction to React", Color = "blue", Start = DateTime.UtcNow.AddDays(1) }
            };
            var documentRequests = new List<DocumentRequest>();

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId)).ReturnsAsync(project);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(allTrainees);
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(curriculums);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(documentRequests);

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Profile.ShouldNotBeNull();
            result.Data.Profile.FirstName.ShouldBe("John");
            result.Data.Profile.BatchId.ShouldBe(batchId);
            result.Data.Profile.ProjectId.ShouldBe(projectId);
            result.Data.Batch.ShouldNotBeNull();
            result.Data.Project.ShouldNotBeNull();
            result.Data.Sessions.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_TraineeNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = 999;
            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync((Trainee?)null);

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Trainee not found.");
        }

        [Fact]
        public async Task Handle_TraineeWithoutProject_ReturnsNullProject()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch 2024",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Project.ShouldBeNull();
            result.Data.Documents.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_BatchNotStarted_CalculatesStatusCorrectly()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Future Batch",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(120),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Batch.Status.ShouldBe("Not Started");
        }

        [Fact]
        public async Task Handle_BatchCompleted_CalculatesStatusCorrectly()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Completed Batch",
                StartDate = DateTime.UtcNow.AddDays(-120),
                EndDate = DateTime.UtcNow.AddDays(-1),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Batch.Status.ShouldBe("Completed");
        }

        [Fact]
        public async Task Handle_BatchOngoing_CalculatesStatusCorrectly()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Current Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Batch.Status.ShouldBe("Ongoing");
        }

        [Fact]
        public async Task Handle_TraineeWithResults_CalculatesScoresAndRank()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee1 = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>
                {
                    new Result { ObtainedMark = 80, Assessment = new Assessment { Type = AssessmentType.TechFundamentals } },
                    new Result { ObtainedMark = 90, Assessment = new Assessment { Type = AssessmentType.Specialisation } }
                }
            };

            var trainee2 = new Trainee
            {
                Id = 2,
                UserId = 2,
                BatchId = batchId,
                Results = new List<Result>
                {
                    new Result { ObtainedMark = 95, Assessment = new Assessment { Type = AssessmentType.TechFundamentals } }
                }
            };

            var allTrainees = new List<Trainee> { trainee1, trainee2 };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee1);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(allTrainees);
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Scores.ShouldNotBeNull();
            result.Data.Scores.Average.ShouldBe(85); // (80 + 90) / 2
            result.Data.Scores.Rank.ShouldBe(2); // trainee2 has higher average
        }

        [Fact]
        public async Task Handle_TraineeWithNoResults_ReturnsZeroScores()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Scores.Average.ShouldBe(0);
            result.Data.Scores.Rank.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_ProjectWithMultipleTeamMembers_ListsAllMembers()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;
            var projectId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var project = new Project
            {
                Id = projectId,
                ProjectName = "Team Project",
                Status = ProjectStatus.Live,
                Progress = 50,
                Technology = "React, Node.js",
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam { TraineeId = 1, Trainee = new Trainee { User = new User { Username = "John Doe" } } },
                    new ProjectTeam { TraineeId = 2, Trainee = new Trainee { User = new User { Username = "Jane Smith" } } },
                    new ProjectTeam { TraineeId = 3, Trainee = new Trainee { User = new User { Username = "Bob Johnson" } } }
                }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam { ProjectId = projectId, TraineeId = 1 }
                },
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId)).ReturnsAsync(project);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(new List<DocumentRequest>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Project.Team.Number.ShouldBe(3);
            result.Data.Project.Team.Members.Count.ShouldBe(3);
            result.Data.Project.Team.Members.ShouldContain("John Doe");
            result.Data.Project.Team.Members.ShouldContain("Jane Smith");
            result.Data.Project.Team.Members.ShouldContain("Bob Johnson");
        }

        [Fact]
        public async Task Handle_ProjectWithDocumentRequirements_ReturnsDocuments()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;
            var projectId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project",
                Status = ProjectStatus.Live,
                Progress = 50,
                Technology = "React",
                ProjectTeams = new List<ProjectTeam>()
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam { ProjectId = projectId, TraineeId = 1 }
                },
                Results = new List<Result>()
            };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = projectId,
                    DocumentId = 1,
                    Document = new IlpRepoBackend.Domain.Entities.Documents
                    {
                        Id = 1,
                        Name = "Requirements Doc",
                        FileType = "PDF",
                        UploadDate = DateTime.UtcNow.AddDays(-5),
                        Link = "http://example.com/doc1.pdf"
                    }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId)).ReturnsAsync(project);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(projectId)).ReturnsAsync(documentRequests);

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Documents.Count.ShouldBe(1);
            result.Data.Documents[0].Title.ShouldBe("Requirements Doc");
            result.Data.Documents[0].Type.ShouldBe("PDF");
        }

        [Fact]
        public async Task Handle_MultipleSessions_ReturnsAllSessions()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            var curriculums = new List<CurriculumEntity>
            {
                new CurriculumEntity { Id = 1, Title = "React Basics", Color = "blue", Start = DateTime.UtcNow.AddDays(1) },
                new CurriculumEntity { Id = 2, Title = "Node.js Fundamentals", Color = "green", Start = DateTime.UtcNow.AddDays(2) },
                new CurriculumEntity { Id = 3, Title = "Database Design", Color = "red", Start = DateTime.UtcNow.AddDays(3) }
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(curriculums);

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Sessions.Count.ShouldBe(3);
            result.Data.Sessions.ShouldContain(s => s.Title == "React Basics");
            result.Data.Sessions.ShouldContain(s => s.Title == "Node.js Fundamentals");
            result.Data.Sessions.ShouldContain(s => s.Title == "Database Design");
        }

        [Fact]
        public async Task Handle_CallsAllRepositories()
        {
            // Arrange
            var userId = 1;
            var batchId = 1;

            var user = new User { Id = userId, Username = "John Doe" };
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainee = new Trainee
            {
                Id = 1,
                UserId = userId,
                BatchId = batchId,
                User = user,
                Batch = batch,
                ProjectTeams = new List<ProjectTeam>(),
                Results = new List<Result>()
            };

            _traineeRepositoryMock.Setup(x => x.GetByUserIdWithDetailsAsync(userId)).ReturnsAsync(trainee);
            _traineeRepositoryMock.Setup(x => x.GetTraineesWithResultsByBatchIdAsync(batchId)).ReturnsAsync(new List<Trainee> { trainee });
            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(new List<CurriculumEntity>());

            var query = new GetTraineeDashboardQuery { UserId = userId };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _traineeRepositoryMock.Verify(x => x.GetByUserIdWithDetailsAsync(userId), Times.Once);
            _traineeRepositoryMock.Verify(x => x.GetTraineesWithResultsByBatchIdAsync(batchId), Times.Once);
            _curriculumRepositoryMock.Verify(x => x.GetByBatchIdAsync(batchId), Times.Once);
        }
    }
}
