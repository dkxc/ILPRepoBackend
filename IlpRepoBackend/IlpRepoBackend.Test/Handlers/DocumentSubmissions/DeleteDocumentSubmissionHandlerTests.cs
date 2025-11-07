using IlpRepoBackend.Application.Command.DocumentSubmissions;
using IlpRepoBackend.Application.Handler.DocumentSubmissions;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Interfaces;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.DocumentSubmissions
{
    public class DeleteDocumentSubmissionHandlerTests
    {
        private readonly Mock<IDocumentSubmissionRepository> _documentSubmissionRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<ILogger<DeleteDocumentSubmissionHandler>> _loggerMock;
        private readonly DeleteDocumentSubmissionHandler _handler;

        public DeleteDocumentSubmissionHandlerTests()
        {
            _documentSubmissionRepositoryMock = new Mock<IDocumentSubmissionRepository>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _loggerMock = new Mock<ILogger<DeleteDocumentSubmissionHandler>>();
            _handler = new DeleteDocumentSubmissionHandler(
                _documentSubmissionRepositoryMock.Object,
                _fileStorageServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSubmission_DeletesSuccessfully()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = "https://storage.example.com/documents/document.pdf"
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeTrue();
            result.Message.ShouldContain("deleted successfully");
            _fileStorageServiceMock.Verify(x => x.DeleteFileAsync("document.pdf"), Times.Once);
            _documentSubmissionRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_SubmissionNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteDocumentSubmissionCommand(999);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((DocumentSubmission)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _documentSubmissionRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SubmissionWithoutLink_DeletesFromDatabaseOnly()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = null
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _documentSubmissionRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_FileDeleteFails_ContinuesWithDatabaseDelete()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = "https://storage.example.com/documents/document.pdf"
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("File not found in storage"));
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("deleted successfully");
            _documentSubmissionRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_DatabaseDeleteFails_ReturnsFailure()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = "https://storage.example.com/documents/document.pdf"
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to delete");
        }

        [Fact]
        public async Task Handle_ExtractsFileNameCorrectly()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = "https://storage.example.com/folder/subfolder/my_document.pdf"
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _fileStorageServiceMock.Setup(x => x.DeleteFileAsync("my_document.pdf"))
                .ReturnsAsync(true);
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _fileStorageServiceMock.Verify(x => x.DeleteFileAsync("my_document.pdf"), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptySubmissionLink_SkipsFileDelete()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = ""
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var submission = new DocumentSubmission
            {
                Id = 1,
                FileName = "document.pdf",
                SubmissionLink = "https://storage.example.com/document.pdf"
            };

            var command = new DeleteDocumentSubmissionCommand(1);

            _documentSubmissionRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(submission);
            _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _documentSubmissionRepositoryMock.Setup(x => x.DeleteAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to delete document submission");
        }
    }
}
