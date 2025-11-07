using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.AdminDashboard;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.AdminDashboard
{
    public class GetProjectsByBatchIdQueryHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjecTeamRepository> _projectTeamRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly GetProjectsByBatchIdQueryHandler _handler;

        public GetProjectsByBatchIdQueryHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectTeamRepositoryMock = new Mock<IProjecTeamRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _handler = new GetProjectsByBatchIdQueryHandler(
                _projectRepositoryMock.Object,
                _projectTeamRepositoryMock.Object,
                _traineeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchId_ReturnsProjectsWithDetails()
        {
            // Arrange
            var batchId = 1;
            
            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "lead@example.com" },
                new Trainee { Id = 2, BatchId = batchId, Email = "member1@example.com" },
                new Trainee { Id = 3, BatchId = batchId, Email = "member2@example.com" }
            };

            var projectTeams = new List<ProjectTeam>
            {
                new ProjectTeam { Id = 1, ProjectId = 1, TraineeId = 1, Role = ProjectRole.TeamLead },
                new ProjectTeam { Id = 2, ProjectId = 1, TraineeId = 2, Role = ProjectRole.Trainee },
                new ProjectTeam { Id = 3, ProjectId = 2, TraineeId = 3, Role = ProjectRole.Trainee }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "E-Commerce App", Technology = "React, Node.js", Status = ProjectStatus.Live },
                new Project { Id = 2, ProjectName = "Mobile Banking", Technology = "Flutter", Status = ProjectStatus.Completed }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            
            var project1 = result.FirstOrDefault(p => p.ProjectId == 1);
            project1.ShouldNotBeNull();
            project1.ProjectName.ShouldBe("E-Commerce App");
            project1.TeamLeadName.ShouldBe("lead@example.com");
            project1.NoOfTrainees.ShouldBe(2);
            project1.Technology.ShouldBe("React, Node.js");
            project1.Status.ShouldBe("Live");
        }

        [Fact]
        public async Task Handle_NoTraineesInBatch_ReturnsEmptyList()
        {
            // Arrange
            var batchId = 999;

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ProjectTeam>());
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Project>());

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_NoProjectsForBatch_ReturnsEmptyList()
        {
            // Arrange
            var batchId = 1;

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "trainee@example.com" }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ProjectTeam>());
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Project>());

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_ProjectWithoutTeamLead_ReturnsEmptyTeamLeadName()
        {
            // Arrange
            var batchId = 1;

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "member1@example.com" },
                new Trainee { Id = 2, BatchId = batchId, Email = "member2@example.com" }
            };

            var projectTeams = new List<ProjectTeam>
            {
                new ProjectTeam { Id = 1, ProjectId = 1, TraineeId = 1, Role = ProjectRole.Trainee },
                new ProjectTeam { Id = 2, ProjectId = 1, TraineeId = 2, Role = ProjectRole.Trainee }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project Without Lead", Technology = "Java", Status = ProjectStatus.Live }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1);
            result[0].TeamLeadName.ShouldBe(string.Empty);
            result[0].NoOfTrainees.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_MultipleProjectsWithDifferentStatuses_ReturnsAll()
        {
            // Arrange
            var batchId = 1;

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "lead1@example.com" },
                new Trainee { Id = 2, BatchId = batchId, Email = "lead2@example.com" },
                new Trainee { Id = 3, BatchId = batchId, Email = "lead3@example.com" }
            };

            var projectTeams = new List<ProjectTeam>
            {
                new ProjectTeam { Id = 1, ProjectId = 1, TraineeId = 1, Role = ProjectRole.TeamLead },
                new ProjectTeam { Id = 2, ProjectId = 2, TraineeId = 2, Role = ProjectRole.TeamLead },
                new ProjectTeam { Id = 3, ProjectId = 3, TraineeId = 3, Role = ProjectRole.TeamLead }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1", Technology = "React", Status = ProjectStatus.Live },
                new Project { Id = 2, ProjectName = "Project 2", Technology = "Angular", Status = ProjectStatus.Completed },
                new Project { Id = 3, ProjectName = "Project 3", Technology = "Vue", Status = ProjectStatus.NotLive }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(3);
            result.ShouldContain(p => p.Status == "Live");
            result.ShouldContain(p => p.Status == "Completed");
            result.ShouldContain(p => p.Status == "NotLive");
        }

        [Fact]
        public async Task Handle_ProjectWithNullTechnology_ReturnsNA()
        {
            // Arrange
            var batchId = 1;

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "lead@example.com" }
            };

            var projectTeams = new List<ProjectTeam>
            {
                new ProjectTeam { Id = 1, ProjectId = 1, TraineeId = 1, Role = ProjectRole.TeamLead }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project Without Tech", Technology = null, Status = ProjectStatus.Live }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1);
            result[0].Technology.ShouldBe("N/A");
        }

        [Fact]
        public async Task Handle_LargeTeamProject_CountsAllMembers()
        {
            // Arrange
            var batchId = 1;

            var trainees = Enumerable.Range(1, 10)
                .Select(i => new Trainee { Id = i, BatchId = batchId, Email = $"trainee{i}@example.com" })
                .ToList();

            var projectTeams = trainees
                .Select((t, index) => new ProjectTeam
                {
                    Id = index + 1,
                    ProjectId = 1,
                    TraineeId = t.Id,
                    Role = index == 0 ? ProjectRole.TeamLead : ProjectRole.Trainee
                })
                .ToList();

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Large Team Project", Technology = "Full Stack", Status = ProjectStatus.Live }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1);
            result[0].NoOfTrainees.ShouldBe(10);
            result[0].TeamLeadName.ShouldBe("trainee1@example.com");
        }

        [Fact]
        public async Task Handle_SubmissionRateIsZero_PlaceholderValue()
        {
            // Arrange
            var batchId = 1;

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "lead@example.com" }
            };

            var projectTeams = new List<ProjectTeam>
            {
                new ProjectTeam { Id = 1, ProjectId = 1, TraineeId = 1, Role = ProjectRole.TeamLead }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Test Project", Technology = "Test", Status = ProjectStatus.Live }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result[0].SubmissionRate.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_FiltersOutProjectsFromOtherBatches()
        {
            // Arrange
            var batchId = 1;

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "batch1@example.com" },
                new Trainee { Id = 2, BatchId = 2, Email = "batch2@example.com" }
            };

            var projectTeams = new List<ProjectTeam>
            {
                new ProjectTeam { Id = 1, ProjectId = 1, TraineeId = 1, Role = ProjectRole.TeamLead },
                new ProjectTeam { Id = 2, ProjectId = 2, TraineeId = 2, Role = ProjectRole.TeamLead }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Batch 1 Project", Technology = "React", Status = ProjectStatus.Live },
                new Project { Id = 2, ProjectName = "Batch 2 Project", Technology = "Angular", Status = ProjectStatus.Live }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projectTeams);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1);
            result[0].ProjectName.ShouldBe("Batch 1 Project");
        }

        [Fact]
        public async Task Handle_CallsAllRepositories()
        {
            // Arrange
            var batchId = 1;

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _projectTeamRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ProjectTeam>());
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Project>());

            var query = new GetProjectsByBatchIdQuery(batchId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _traineeRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            _projectTeamRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            _projectRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }
    }
}
