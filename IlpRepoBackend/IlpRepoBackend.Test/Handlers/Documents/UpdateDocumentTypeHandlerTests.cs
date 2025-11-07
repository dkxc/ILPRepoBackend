using IlpRepoBackend.Application.Command.Documents;
using IlpRepoBackend.Application.Handler.Documents;
using IlpRepoBackend.Domain.Interfaces;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.Documents
{
    public class UpdateDocumentTypeHandlerTests
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<ILogger<UpdateDocumentTypeHandler>> _loggerMock;
        private readonly UpdateDocumentTypeHandler _handler;

        public UpdateDocumentTypeHandlerTests()
        {
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _loggerMock = new Mock<ILogger<UpdateDocumentTypeHandler>>();
            _handler = new UpdateDocumentTypeHandler(
                _documentRepositoryMock.Object,
                _fileStorageServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommandWithoutFile_UpdatesDocumentType()
        {
            // Arrange
            var existingDocument = new DocumentEntity
            {
                Id = 1,
                Name = "Old Name",
                FileType = "pdf",
                Link = "https://example.com/old.pdf",
                UploadDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var command = new UpdateDocumentTypeCommand(1, "Updated Name", null, "docx");

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingDocument);
            _documentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<DocumentEntity>()))
                .ReturnsAsync((DocumentEntity doc) => doc);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(1);
            result.Data.Name.ShouldBe("Updated Name");
            result.Data.FileType.ShouldBe("docx");
            result.Data.Link.ShouldBe("https://example.com/old.pdf");
            _documentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<DocumentEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommandWithFile_UpdatesDocumentTypeAndFile()
        {
            // Arrange
            var existingDocument = new DocumentEntity
            {
                Id = 1,
                Name = "Old Name",
                FileType = "pdf",
                Link = "https://example.com/old.pdf",
                UploadDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(2048);
            fileMock.Setup(f => f.FileName).Returns("new_template.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var command = new UpdateDocumentTypeCommand(1, "Updated Name", fileMock.Object, "pdf");

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingDocument);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://storage.example.com/documents/new_template.pdf");
            _documentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<DocumentEntity>()))
                .ReturnsAsync((DocumentEntity doc) => doc);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Name.ShouldBe("Updated Name");
            result.Data.Link.ShouldBe("https://storage.example.com/documents/new_template.pdf");
            _fileStorageServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), "new_template.pdf", "application/pdf"), Times.Once);
            _documentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<DocumentEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DocumentNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateDocumentTypeCommand(999, "Updated Name");

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((DocumentEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _documentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<DocumentEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            // Arrange
            var existingDocument = new DocumentEntity
            {
                Id = 1,
                Name = "Document Name",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            var command = new UpdateDocumentTypeCommand(1, "Updated Name", fileMock.Object);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingDocument);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("empty");
            _documentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<DocumentEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_FileUploadFails_ReturnsFailure()
        {
            // Arrange
            var existingDocument = new DocumentEntity
            {
                Id = 1,
                Name = "Document Name",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("template.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var command = new UpdateDocumentTypeCommand(1, "Updated Name", fileMock.Object);

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingDocument);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Storage service unavailable"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to update document type");
            _documentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<DocumentEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdatesUploadDateWhenFileProvided()
        {
            // Arrange
            var existingDocument = new DocumentEntity
            {
                Id = 1,
                Name = "Document Name",
                UploadDate = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            };

            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("new.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var command = new UpdateDocumentTypeCommand(1, "Updated Name", fileMock.Object);

            var beforeUpdate = DateTime.UtcNow;

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingDocument);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://example.com/new.pdf");
            _documentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<DocumentEntity>()))
                .ReturnsAsync((DocumentEntity doc) => doc);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.UploadDate.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
            result.Data.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var existingDocument = new DocumentEntity
            {
                Id = 1,
                Name = "Document Name",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var command = new UpdateDocumentTypeCommand(1, "Updated Name");

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existingDocument);
            _documentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<DocumentEntity>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to update document type");
        }
    }
}
