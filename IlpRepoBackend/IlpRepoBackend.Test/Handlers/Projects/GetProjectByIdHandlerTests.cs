using AutoMapper;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Handler.Projects;
using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Projects
{
    public class GetProjectByIdHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetProjectByIdHandler _handler;

        public GetProjectByIdHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetProjectByIdHandler(
                _projectRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidProjectId_ReturnsProjectWithBatchInfo()
        {
            // Arrange
            var projectId = 1;
            var batchId = 10;
            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project",
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam
                    {
                        Trainee = new Trainee
                        {
                            Id = 1,
                            BatchId = batchId
                        }
                    }
                }
            };

            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch"
            };

            var projectDto = new ProjectDto
            {
                Id = projectId,
                ProjectName = "Test Project"
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync(project);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<ProjectDto>(project))
                .Returns(projectDto);

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(projectId);
            result.Data.BatchId.ShouldBe(batchId);
            result.Data.BatchName.ShouldBe("Test Batch");
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsFailure()
        {
            // Arrange
            var projectId = 999;
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync((Project?)null);

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            result.Message.ShouldContain(projectId.ToString());
        }

        [Fact]
        public async Task Handle_ProjectWithNoTeams_ReturnsProjectWithoutBatchInfo()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project",
                ProjectTeams = new List<ProjectTeam>()
            };

            var projectDto = new ProjectDto
            {
                Id = projectId,
                ProjectName = "Test Project"
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync(project);
            _mapperMock.Setup(x => x.Map<ProjectDto>(project))
                .Returns(projectDto);

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(projectId);
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ProjectWithNullTeams_ReturnsProjectWithoutBatchInfo()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project",
                ProjectTeams = null
            };

            var projectDto = new ProjectDto
            {
                Id = projectId,
                ProjectName = "Test Project"
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync(project);
            _mapperMock.Setup(x => x.Map<ProjectDto>(project))
                .Returns(projectDto);

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsProjectWithoutBatchName()
        {
            // Arrange
            var projectId = 1;
            var batchId = 10;
            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project",
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam
                    {
                        Trainee = new Trainee { BatchId = batchId }
                    }
                }
            };

            var projectDto = new ProjectDto
            {
                Id = projectId,
                ProjectName = "Test Project"
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync(project);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync((Batch?)null);
            _mapperMock.Setup(x => x.Map<ProjectDto>(project))
                .Returns(projectDto);

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.BatchId.ShouldBe(0); // Default value when batch not found
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            var projectId = 1;
            var exceptionMessage = "Database connection failed";

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ThrowsAsync(new Exception(exceptionMessage));

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error retrieving project");
            result.Message.ShouldContain(exceptionMessage);
        }

        [Fact]
        public async Task Handle_MapperReturnsNull_HandlesGracefully()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project"
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync(project);
            _mapperMock.Setup(x => x.Map<ProjectDto>(project))
                .Returns((ProjectDto?)null);

            var query = new GetProjectByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            // Handler should handle null projectDto gracefully
        }
    }
}
