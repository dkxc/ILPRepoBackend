using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Handler.Projects;
using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.Projects
{
    public class GetProjectDetailsByIdHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly GetProjectDetailsByIdHandler _handler;

        public GetProjectDetailsByIdHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _handler = new GetProjectDetailsByIdHandler(_projectRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidProjectId_ReturnsProjectDetails()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                ProjectName = "Test Project",
                Status = ProjectStatus.Live,
                Progress = 50,
                Technology = "C#, .NET",
                ProjectTeams = new List<ProjectTeam>
                {
                    new ProjectTeam
                    {
                        Trainee = new Trainee
                        {
                            User = new User { Username = "John Doe", Email = "john@example.com" }
                        }
                    }
                },
                ProjectLinks = new List<ProjectLink>
                {
                    new ProjectLink
                    {
                        Id = 1,
                        LinkId = 1,
                        LinkUrl = "https://github.com/test",
                        Link = new Link { Name = "GitHub" }
                    }
                }
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync(project);

            var query = new GetProjectDetailsByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(projectId);
            result.Data.ProjectName.ShouldBe("Test Project");
            result.Data.Status.ShouldBe(ProjectStatus.Live);
            result.Data.Progress.ShouldBe(50);
            result.Data.TechnologyStack.ShouldBe("C#, .NET");
            result.Data.Trainees.Count.ShouldBe(1);
            result.Data.ProjectLinks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsFailure()
        {
            // Arrange
            var projectId = 999;
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ReturnsAsync((Project)null);

            var query = new GetProjectDetailsByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_ProjectWithNoTrainees_ReturnsEmptyTraineesList()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project",
                ProjectTeams = new List<ProjectTeam>()
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1))
                .ReturnsAsync(project);

            var query = new GetProjectDetailsByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Trainees.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_ProjectWithDocumentSubmissions_ReturnsSubmissions()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project",
                DocumentRequests = new List<DocumentRequest>
                {
                    new DocumentRequest
                    {
                        Id = 1,
                        DueDate = DateTime.UtcNow.AddDays(7),
                        DocumentSubmissions = new List<DocumentSubmission>
                        {
                            new DocumentSubmission
                            {
                                Id = 1,
                                FileName = "report.pdf",
                                FileType = "PDF",
                                SubmissionLink = "https://example.com/report.pdf",
                                SubmissionDate = DateTime.UtcNow,
                                Document = new DocumentEntity { Name = "Project Report" }
                            }
                        }
                    }
                }
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1))
                .ReturnsAsync(project);

            var query = new GetProjectDetailsByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.DocumentSubmissions.Count.ShouldBe(1);
            result.Data.DocumentSubmissions[0].FileName.ShouldBe("report.pdf");
        }

        [Fact]
        public async Task Handle_ProjectWithNullTechnology_ReturnsEmptyString()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project",
                Technology = null
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1))
                .ReturnsAsync(project);

            var query = new GetProjectDetailsByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.TechnologyStack.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Handle_ProjectWithNullCollections_ReturnsEmptyLists()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project",
                ProjectTeams = null,
                ProjectLinks = null,
                DocumentRequests = null
            };

            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1))
                .ReturnsAsync(project);

            var query = new GetProjectDetailsByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Trainees.ShouldBeEmpty();
            result.Data.ProjectLinks.ShouldBeEmpty();
            result.Data.DocumentSubmissions.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var projectId = 1;
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(projectId))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetProjectDetailsByIdQuery(projectId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error retrieving project details");
        }
    }
}
