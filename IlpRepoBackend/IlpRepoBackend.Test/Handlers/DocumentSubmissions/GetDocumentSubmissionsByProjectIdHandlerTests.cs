using IlpRepoBackend.Application.Handler.DocumentSubmissions;
using IlpRepoBackend.Application.Query.DocumentSubmissions;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.DocumentSubmissions
{
    public class GetDocumentSubmissionsByProjectIdHandlerTests
    {
        private readonly Mock<IDocumentSubmissionRepository> _documentSubmissionRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ILogger<GetDocumentSubmissionsByProjectIdHandler>> _loggerMock;
        private readonly GetDocumentSubmissionsByProjectIdHandler _handler;

        public GetDocumentSubmissionsByProjectIdHandlerTests()
        {
            _documentSubmissionRepositoryMock = new Mock<IDocumentSubmissionRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _loggerMock = new Mock<ILogger<GetDocumentSubmissionsByProjectIdHandler>>();
            _handler = new GetDocumentSubmissionsByProjectIdHandler(
                _documentSubmissionRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_SubmissionsExist_ReturnsAllSubmissions()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project"
            };

            var submissions = new List<DocumentSubmission>
            {
                new DocumentSubmission
                {
                    Id = 1,
                    FileName = "proposal.pdf",
                    FileType = "PDF",
                    SubmissionLink = "https://example.com/proposal.pdf",
                    SubmissionDate = DateTime.UtcNow.AddDays(-2),
                    Document = new DocumentEntity { Id = 1, Name = "Project Proposal" },
                    DocumentRequest = new DocumentRequest
                    {
                        Id = 1,
                        ProjectId = 1,
                        DueDate = DateTime.UtcNow.AddDays(5)
                    }
                },
                new DocumentSubmission
                {
                    Id = 2,
                    FileName = "report.docx",
                    FileType = "DOCX",
                    SubmissionLink = "https://example.com/report.docx",
                    SubmissionDate = DateTime.UtcNow.AddDays(-1),
                    Document = new DocumentEntity { Id = 2, Name = "Status Report" },
                    DocumentRequest = new DocumentRequest
                    {
                        Id = 2,
                        ProjectId = 1,
                        DueDate = DateTime.UtcNow.AddDays(3)
                    }
                }
            };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentSubmissionRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(submissions);

            var query = new GetDocumentSubmissionsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].FileName.ShouldBe("proposal.pdf");
            result.Data[0].DocumentTypeName.ShouldBe("Project Proposal");
            result.Data[1].FileName.ShouldBe("report.docx");
            result.Data[1].DocumentTypeName.ShouldBe("Status Report");
        }

        [Fact]
        public async Task Handle_LateSubmission_MarksCorrectly()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            var submissions = new List<DocumentSubmission>
            {
                new DocumentSubmission
                {
                    Id = 1,
                    FileName = "late_submission.pdf",
                    FileType = "PDF",
                    SubmissionLink = "https://example.com/late.pdf",
                    SubmissionDate = DateTime.UtcNow,
                    Document = new DocumentEntity { Id = 1, Name = "Project Proposal" },
                    DocumentRequest = new DocumentRequest
                    {
                        Id = 1,
                        ProjectId = 1,
                        DueDate = DateTime.UtcNow.AddDays(-5) // Due date in the past
                    }
                }
            };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentSubmissionRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(submissions);

            var query = new GetDocumentSubmissionsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].IsLateSubmission.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsFailure()
        {
            // Arrange
            _projectRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Project)null);

            var query = new GetDocumentSubmissionsByProjectIdQuery(999);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _documentSubmissionRepositoryMock.Verify(x => x.GetByProjectIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoSubmissions_ReturnsEmptyList()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentSubmissionRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<DocumentSubmission>());

            var query = new GetDocumentSubmissionsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_SubmissionsWithNullProperties_HandlesGracefully()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            var submissions = new List<DocumentSubmission>
            {
                new DocumentSubmission
                {
                    Id = 1,
                    FileName = null,
                    FileType = null,
                    SubmissionLink = null,
                    SubmissionDate = DateTime.UtcNow,
                    Document = new DocumentEntity { Id = 1, Name = "Document" },
                    DocumentRequest = new DocumentRequest
                    {
                        Id = 1,
                        ProjectId = 1,
                        DueDate = DateTime.UtcNow.AddDays(5)
                    }
                }
            };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentSubmissionRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(submissions);

            var query = new GetDocumentSubmissionsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].FileName.ShouldBe("Unknown");
            result.Data[0].FileType.ShouldBe("Unknown");
        }

        [Fact]
        public async Task Handle_SubmissionWithoutDocument_IsExcluded()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            var submissions = new List<DocumentSubmission>
            {
                new DocumentSubmission
                {
                    Id = 1,
                    FileName = "file.pdf",
                    Document = null, // No document
                    DocumentRequest = new DocumentRequest { Id = 1 }
                },
                new DocumentSubmission
                {
                    Id = 2,
                    FileName = "valid_file.pdf",
                    Document = new DocumentEntity { Id = 1, Name = "Valid Document" },
                    DocumentRequest = new DocumentRequest
                    {
                        Id = 2,
                        ProjectId = 1,
                        DueDate = DateTime.UtcNow.AddDays(5)
                    }
                }
            };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentSubmissionRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(submissions);

            var query = new GetDocumentSubmissionsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].DocumentTypeName.ShouldBe("Valid Document");
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentSubmissionRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetDocumentSubmissionsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to retrieve document submissions");
        }
    }
}
