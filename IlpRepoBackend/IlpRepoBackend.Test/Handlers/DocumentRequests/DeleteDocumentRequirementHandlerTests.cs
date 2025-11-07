using IlpRepoBackend.Application.Command.DocumentRequests;
using IlpRepoBackend.Application.Handler.DocumentRequests;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.DocumentRequests
{
    public class DeleteDocumentRequirementHandlerTests
    {
        private readonly Mock<IDocumentRequestRepository> _documentRequestRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<ILogger<DeleteDocumentRequirementHandler>> _loggerMock;
        private readonly DeleteDocumentRequirementHandler _handler;

        public DeleteDocumentRequirementHandlerTests()
        {
            _documentRequestRepositoryMock = new Mock<IDocumentRequestRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _loggerMock = new Mock<ILogger<DeleteDocumentRequirementHandler>>();
            _handler = new DeleteDocumentRequirementHandler(
                _documentRequestRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _documentRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_DeletesAllRequirements()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" },
                new Project { Id = 2, ProjectName = "Project B" }
            };

            var existingRequests = new List<DocumentRequest>
            {
                new DocumentRequest { Id = 1, ProjectId = 1, DocumentId = 1 },
                new DocumentRequest { Id = 2, ProjectId = 2, DocumentId = 1 }
            };

            var command = new DeleteDocumentRequirementCommand(1, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(existingRequests);
            _documentRequestRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeTrue();
            result.Message.ShouldContain("deleted successfully");
            result.Message.ShouldContain("2");
            _documentRequestRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _documentRequestRepositoryMock.Verify(x => x.DeleteAsync(2), Times.Once);
        }

        [Fact]
        public async Task Handle_DocumentTypeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteDocumentRequirementCommand(999, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((DocumentEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _documentRequestRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoProjectsInBatch_ReturnsFailure()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };

            var command = new DeleteDocumentRequirementCommand(1, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(new List<Project>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No projects found");
            _documentRequestRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoExistingRequirements_ReturnsFailure()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" }
            };

            var command = new DeleteDocumentRequirementCommand(1, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<DocumentRequest>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No document requirement found");
            _documentRequestRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_PartialDeleteFailure_CountsSuccessfulDeletes()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" },
                new Project { Id = 2, ProjectName = "Project B" }
            };

            var existingRequests = new List<DocumentRequest>
            {
                new DocumentRequest { Id = 1, ProjectId = 1, DocumentId = 1 },
                new DocumentRequest { Id = 2, ProjectId = 2, DocumentId = 1 }
            };

            var command = new DeleteDocumentRequirementCommand(1, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(existingRequests);
            _documentRequestRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);
            _documentRequestRepositoryMock.Setup(x => x.DeleteAsync(2))
                .ReturnsAsync(false); // One fails

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("1");
        }

        [Fact]
        public async Task Handle_MultipleProjects_DeletesAllRequests()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Status Report" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" },
                new Project { Id = 2, ProjectName = "Project B" },
                new Project { Id = 3, ProjectName = "Project C" },
                new Project { Id = 4, ProjectName = "Project D" }
            };

            var existingRequests = new List<DocumentRequest>
            {
                new DocumentRequest { Id = 1, ProjectId = 1, DocumentId = 1 },
                new DocumentRequest { Id = 2, ProjectId = 2, DocumentId = 1 },
                new DocumentRequest { Id = 3, ProjectId = 3, DocumentId = 1 },
                new DocumentRequest { Id = 4, ProjectId = 4, DocumentId = 1 }
            };

            var command = new DeleteDocumentRequirementCommand(1, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(existingRequests);
            _documentRequestRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("4");
            _documentRequestRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Exactly(4));
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" }
            };

            var command = new DeleteDocumentRequirementCommand(1, 1);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to delete document requirement");
        }
    }
}
