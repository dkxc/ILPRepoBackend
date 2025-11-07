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
    public class SetDocumentRequirementHandlerTests
    {
        private readonly Mock<IDocumentRequestRepository> _documentRequestRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<ILogger<SetDocumentRequirementHandler>> _loggerMock;
        private readonly SetDocumentRequirementHandler _handler;

        public SetDocumentRequirementHandlerTests()
        {
            _documentRequestRepositoryMock = new Mock<IDocumentRequestRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _loggerMock = new Mock<ILogger<SetDocumentRequirementHandler>>();
            _handler = new SetDocumentRequirementHandler(
                _documentRequestRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _documentRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesDocumentRequests()
        {
            // Arrange
            var documentType = new DocumentEntity
            {
                Id = 1,
                Name = "Project Proposal"
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" },
                new Project { Id = 2, ProjectName = "Project B" }
            };

            var command = new SetDocumentRequirementCommand(1, 1, DateTime.UtcNow.AddDays(14));

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<DocumentRequest>());
            _documentRequestRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentRequest>()))
                .ReturnsAsync((DocumentRequest dr) =>
                {
                    dr.Id = 1;
                    return dr;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.DocumentTypeId.ShouldBe(1);
            result.Data.DocumentTypeName.ShouldBe("Project Proposal");
            result.Data.BatchId.ShouldBe(1);
            result.Data.TotalProjectsAffected.ShouldBe(2);
            result.Data.ProjectIds.Count.ShouldBe(2);
            _documentRequestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentRequest>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_DocumentTypeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new SetDocumentRequirementCommand(999, 1, DateTime.UtcNow.AddDays(14));

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((DocumentEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _documentRequestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentRequest>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoProjectsInBatch_ReturnsFailure()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };

            var command = new SetDocumentRequirementCommand(1, 1, DateTime.UtcNow.AddDays(14));

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
            _documentRequestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentRequest>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateRequirement_ReturnsFailure()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" }
            };

            var existingRequests = new List<DocumentRequest>
            {
                new DocumentRequest { Id = 1, ProjectId = 1, DocumentId = 1 }
            };

            var command = new SetDocumentRequirementCommand(1, 1, DateTime.UtcNow.AddDays(14));

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(existingRequests);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
            _documentRequestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentRequest>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MultipleProjects_CreatesMultipleRequests()
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

            var command = new SetDocumentRequirementCommand(1, 1, DateTime.UtcNow.AddDays(7));

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<DocumentRequest>());
            _documentRequestRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentRequest>()))
                .ReturnsAsync((DocumentRequest dr) =>
                {
                    dr.Id = 1;
                    return dr;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.TotalProjectsAffected.ShouldBe(4);
            result.Data.ProjectIds.Count.ShouldBe(4);
            _documentRequestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentRequest>()), Times.Exactly(4));
        }

        [Fact]
        public async Task Handle_SetsDueDateCorrectly()
        {
            // Arrange
            var documentType = new DocumentEntity { Id = 1, Name = "Project Proposal" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project A" }
            };

            var dueDate = DateTime.UtcNow.AddDays(30);
            var command = new SetDocumentRequirementCommand(1, 1, dueDate);

            DocumentRequest capturedRequest = null;

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<DocumentRequest>());
            _documentRequestRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentRequest>()))
                .Callback<DocumentRequest>(dr => capturedRequest = dr)
                .ReturnsAsync((DocumentRequest dr) =>
                {
                    dr.Id = 1;
                    return dr;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.DueDate.ShouldBe(dueDate);
            capturedRequest.ShouldNotBeNull();
            capturedRequest.DueDate.ShouldBe(dueDate);
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

            var command = new SetDocumentRequirementCommand(1, 1, DateTime.UtcNow.AddDays(14));

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(documentType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(projects);
            _documentRequestRepositoryMock.Setup(x => x.GetByDocumentIdAndProjectIdsAsync(1, It.IsAny<List<int>>()))
                .ReturnsAsync(new List<DocumentRequest>());
            _documentRequestRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentRequest>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to set document requirement");
        }
    }
}
