using AutoMapper;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Handler.Projects;
using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Projects
{
    public class GetAllProjectsHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAllProjectsHandler _handler;

        public GetAllProjectsHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetAllProjectsHandler(
                _projectRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_ProjectsExist_ReturnsAllProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1", Status = ProjectStatus.Live },
                new Project { Id = 2, ProjectName = "Project 2", Status = ProjectStatus.NotLive }
            };

            var projectDtos = new List<ProjectDto>
            {
                new ProjectDto { Id = 1, ProjectName = "Project 1" },
                new ProjectDto { Id = 2, ProjectName = "Project 2" }
            };

            _projectRepositoryMock.Setup(x => x.GetAllProjectsWithDetailsAsync()).ReturnsAsync(projects);
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>()))
                .Returns((Project p) => projectDtos.First(dto => dto.Id == p.Id));

            var query = new GetAllProjectsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_NoProjects_ReturnsEmptyList()
        {
            // Arrange
            _projectRepositoryMock.Setup(x => x.GetAllProjectsWithDetailsAsync())
                .ReturnsAsync(new List<Project>());

            var query = new GetAllProjectsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
            result.Message.ShouldContain("No projects found");
        }

        [Fact]
        public async Task Handle_FilterByStatus_ReturnsFilteredProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, Status = ProjectStatus.Live },
                new Project { Id = 2, Status = ProjectStatus.Completed },
                new Project { Id = 3, Status = ProjectStatus.Live }
            };

            _projectRepositoryMock.Setup(x => x.GetAllProjectsWithDetailsAsync()).ReturnsAsync(projects);
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>()))
                .Returns((Project p) => new ProjectDto { Id = p.Id, Status = p.Status });

            var query = new GetAllProjectsQuery { Status = "Live" };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            result.Data.All(p => p.Status == ProjectStatus.Live).ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_FilterByTechnology_ReturnsFilteredProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, Technology = ".NET Core" },
                new Project { Id = 2, Technology = "Java" },
                new Project { Id = 3, Technology = ".NET Core" }
            };

            _projectRepositoryMock.Setup(x => x.GetAllProjectsWithDetailsAsync()).ReturnsAsync(projects);
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>()))
                .Returns((Project p) => new ProjectDto { Id = p.Id, Technology = p.Technology });

            var query = new GetAllProjectsQuery { Technology = ".NET" };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            _projectRepositoryMock.Setup(x => x.GetAllProjectsWithDetailsAsync())
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetAllProjectsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error retrieving projects");
        }

        [Fact]
        public async Task Handle_GetsBatchInformation_ForProjects()
        {
            // Arrange
            var trainee = new Trainee { Id = 1, BatchId = 1 };
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test",
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam { Trainee = trainee }
                }
            };

            var batch = new Batch { Id = 1, BatchName = "Batch 1" };

            _projectRepositoryMock.Setup(x => x.GetAllProjectsWithDetailsAsync())
                .ReturnsAsync(new List<Project> { project });
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>()))
                .Returns(new ProjectDto { Id = 1 });

            var query = new GetAllProjectsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        }
    }
}
