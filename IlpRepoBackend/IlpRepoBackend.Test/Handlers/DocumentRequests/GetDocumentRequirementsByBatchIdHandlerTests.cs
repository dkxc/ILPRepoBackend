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
    public class GetDocumentRequirementsByBatchIdHandlerTests
    {
        private readonly Mock<IDocumentRequestRepository> _documentRequestRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<ILogger<GetDocumentRequirementsByBatchIdHandler>> _loggerMock;
        private readonly GetDocumentRequirementsByBatchIdHandler _handler;

        public GetDocumentRequirementsByBatchIdHandlerTests()
        {
            _documentRequestRepositoryMock = new Mock<IDocumentRequestRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _loggerMock = new Mock<ILogger<GetDocumentRequirementsByBatchIdHandler>>();
            _handler = new GetDocumentRequirementsByBatchIdHandler(
                _documentRequestRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _documentRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_RequirementsExist_ReturnsGroupedByDocumentType()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" },
                new Project { Id = 2, ProjectName = "Project B" }
            };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = 1,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    RequestDate = DateTime.UtcNow
                },
                new DocumentRequest
                {
                    Id = 2,
                    ProjectId = 2,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    RequestDate = DateTime.UtcNow
                }
            };

            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };

            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<DocumentRequest> { documentRequests[0] });
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(2))
                .ReturnsAsync(new List<DocumentRequest> { documentRequests[1] });
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);

            var query = new GetDocumentRequirementsByBatchIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(1);
            result.Data[0].DocumentTypeName.ShouldBe("Project Proposal");
            result.Data[0].BatchId.ShouldBe(1);
            result.Data[0].TotalProjectsAffected.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_NoProjectsInBatch_ReturnsEmptyList()
        {
            // Arrange
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(new List<Project>());

            var query = new GetDocumentRequirementsByBatchIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(0);
            result.Message.ShouldContain("No projects found");
        }

        [Fact]
        public async Task Handle_MultipleDocumentTypes_ReturnsAllGrouped()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" },
                new Project { Id = 2, ProjectName = "Project B" }
            };

            var documentRequests1 = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 1,
                    ProjectId = 1,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    RequestDate = DateTime.UtcNow
                },
                new DocumentRequest
                {
                    Id = 2,
                    ProjectId = 1,
                    DocumentId = 2,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    RequestDate = DateTime.UtcNow
                }
            };

            var documentRequests2 = new List<DocumentRequest>
            {
                new DocumentRequest
                {
                    Id = 3,
                    ProjectId = 2,
                    DocumentId = 1,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    RequestDate = DateTime.UtcNow
                },
                new DocumentRequest
                {
                    Id = 4,
                    ProjectId = 2,
                    DocumentId = 2,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    RequestDate = DateTime.UtcNow
                }
            };

            var doc1 = new DocumentEntity { Id = 1, Name = "Proposal" };
            var doc2 = new DocumentEntity { Id = 2, Name = "Report" };

            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests1);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(2))
                .ReturnsAsync(documentRequests2);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doc1);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(doc2);

            var query = new GetDocumentRequirementsByBatchIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            result.Data.Any(d => d.DocumentTypeName == "Proposal").ShouldBeTrue();
            result.Data.Any(d => d.DocumentTypeName == "Report").ShouldBeTrue();
            result.Data.All(d => d.TotalProjectsAffected == 2).ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_ResultsSortedByDocumentTypeName()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" }
            };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest { Id = 1, ProjectId = 1, DocumentId = 3, DueDate = DateTime.UtcNow, RequestDate = DateTime.UtcNow },
                new DocumentRequest { Id = 2, ProjectId = 1, DocumentId = 1, DueDate = DateTime.UtcNow, RequestDate = DateTime.UtcNow },
                new DocumentRequest { Id = 3, ProjectId = 1, DocumentId = 2, DueDate = DateTime.UtcNow, RequestDate = DateTime.UtcNow }
            };

            var doc1 = new DocumentEntity { Id = 1, Name = "Alpha" };
            var doc2 = new DocumentEntity { Id = 2, Name = "Beta" };
            var doc3 = new DocumentEntity { Id = 3, Name = "Zeta" };

            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doc1);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(doc2);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(doc3);

            var query = new GetDocumentRequirementsByBatchIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            result.Data[0].DocumentTypeName.ShouldBe("Alpha");
            result.Data[1].DocumentTypeName.ShouldBe("Beta");
            result.Data[2].DocumentTypeName.ShouldBe("Zeta");
        }

        [Fact]
        public async Task Handle_RequestsWithoutDocumentId_AreExcluded()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" }
            };

            var documentRequests = new List<DocumentRequest>
            {
                new DocumentRequest { Id = 1, ProjectId = 1, DocumentId = null, DueDate = DateTime.UtcNow, RequestDate = DateTime.UtcNow },
                new DocumentRequest { Id = 2, ProjectId = 1, DocumentId = 1, DueDate = DateTime.UtcNow, RequestDate = DateTime.UtcNow }
            };

            var doc = new DocumentEntity { Id = 1, Name = "Valid Document" };

            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(documentRequests);
            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(doc);

            var query = new GetDocumentRequirementsByBatchIdQuery(1);

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
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetDocumentRequirementsByBatchIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to retrieve document requirements");
        }
    }
}
