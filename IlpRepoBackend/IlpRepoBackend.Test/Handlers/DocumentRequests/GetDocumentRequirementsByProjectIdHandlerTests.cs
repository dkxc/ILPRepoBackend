using IlpRepoBackend.Application.Handler.DocumentRequests;
using IlpRepoBackend.Application.Query.DocumentRequests;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.DocumentRequests
{
    public class GetDocumentRequirementsByProjectIdHandlerTests
    {
        private readonly Mock<IDocumentRequestRepository> _documentRequestRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<ILogger<GetDocumentRequirementsByProjectIdHandler>> _loggerMock;
        private readonly GetDocumentRequirementsByProjectIdHandler _handler;

        public GetDocumentRequirementsByProjectIdHandlerTests()
        {
            _documentRequestRepositoryMock = new Mock<IDocumentRequestRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _loggerMock = new Mock<ILogger<GetDocumentRequirementsByProjectIdHandler>>();
            _handler = new GetDocumentRequirementsByProjectIdHandler(
                _documentRequestRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _documentRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_RequirementsExist_ReturnsAllRequirements()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                ProjectName = "Test Project"
            };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = 1,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    RequestDate = DateTime.UtcNow.AddDays(-7),
                    DocumentSubmissions = new List<DocumentSubmission>()
                },
                new DocumentRequest
                {
                    Id = 2,
                    ProjectId = 1,
                    DocumentId = 2,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    RequestDate = DateTime.UtcNow.AddDays(-5),
                    DocumentSubmissions = new List<DocumentSubmission>
                    {
                        new DocumentSubmission { Id = 1, SubmissionDate = DateTime.UtcNow.AddDays(-1) }
                    }
                }
            };

            var documentType1 = new DocumentEntity { Id = 1, Name = "Project Proposal", Link = "https://example.com/template1.pdf" };
            var documentType2 = new DocumentEntity { Id = 2, Name = "Status Report", Link = "https://example.com/template2.pdf" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType1);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(documentType2);

            var query = new GetDocumentRequirementsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            // Results are sorted by due date ascending, so Status Report (7 days) comes before Project Proposal (14 days)
            result.Data[0].DocumentTypeName.ShouldBe("Status Report");
            result.Data[0].HasSubmission.ShouldBeTrue();
            result.Data[1].DocumentTypeName.ShouldBe("Project Proposal");
            result.Data[1].HasSubmission.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsFailure()
        {
            // Arrange
            _projectRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Project)null);

            var query = new GetDocumentRequirementsByProjectIdQuery(999);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_NoRequirements_ReturnsEmptyList()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<DocumentRequest>());

            var query = new GetDocumentRequirementsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(0);
            result.Message.ShouldContain("No document requirements found");
        }

        [Fact]
        public async Task Handle_OverdueRequirements_SortsOverdueFirst()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = 1,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(30), // Future
                    RequestDate = DateTime.UtcNow,
                    DocumentSubmissions = new List<DocumentSubmission>()
                },
                new DocumentRequest
                {
                    Id = 2,
                    ProjectId = 1,
                    DocumentId = 2,
                    DueDate = DateTime.UtcNow.AddDays(-5), // Overdue
                    RequestDate = DateTime.UtcNow.AddDays(-20),
                    DocumentSubmissions = new List<DocumentSubmission>()
                },
                new DocumentRequest
                {
                    Id = 3,
                    ProjectId = 1,
                    DocumentId = 3,
                    DueDate = DateTime.UtcNow.AddDays(7), // Future
                    RequestDate = DateTime.UtcNow,
                    DocumentSubmissions = new List<DocumentSubmission>()
                }
            };

            var doc1 = new DocumentEntity { Id = 1, Name = "Document 1" };
            var doc2 = new DocumentEntity { Id = 2, Name = "Overdue Document" };
            var doc3 = new DocumentEntity { Id = 3, Name = "Document 3" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doc1);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(doc2);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(doc3);

            var query = new GetDocumentRequirementsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            result.Data[0].IsOverdue.ShouldBeTrue();
            result.Data[0].DocumentTypeName.ShouldBe("Overdue Document");
        }

        [Fact]
        public async Task Handle_WithSubmissions_TracksLastSubmissionDate()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            var lastSubmissionDate = DateTime.UtcNow.AddDays(-2);
            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = 1,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    RequestDate = DateTime.UtcNow.AddDays(-10),
                    DocumentSubmissions = new List<DocumentSubmission>
                    {
                        new DocumentSubmission { Id = 1, SubmissionDate = DateTime.UtcNow.AddDays(-5) },
                        new DocumentSubmission { Id = 2, SubmissionDate = lastSubmissionDate },
                        new DocumentSubmission { Id = 3, SubmissionDate = DateTime.UtcNow.AddDays(-7) }
                    }
                }
            };

            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);

            var query = new GetDocumentRequirementsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].HasSubmission.ShouldBeTrue();
            result.Data[0].LastSubmissionDate.ShouldBe(lastSubmissionDate);
        }

        [Fact]
        public async Task Handle_RequestWithoutDocumentId_IsSkipped()
        {
            // Arrange
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = 1,
                    DocumentId = null, // No document ID
                    DueDate = DateTime.UtcNow.AddDays(7),
                    DocumentSubmissions = new List<DocumentSubmission>()
                },
                new DocumentRequest
                {
                    Id = 2,
                    ProjectId = 1,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    DocumentSubmissions = new List<DocumentSubmission>()
                }
            };

            var documentType = new DocumentEntity { Id = 1, Name = "Valid Document" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(project);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);

            var query = new GetDocumentRequirementsByProjectIdQuery(1);

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
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetDocumentRequirementsByProjectIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to retrieve document requirements");
        }
    }
}
