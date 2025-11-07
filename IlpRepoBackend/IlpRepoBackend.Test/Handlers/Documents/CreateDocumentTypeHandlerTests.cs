using IlpRepoBackend.Application.Command.Documents;
using IlpRepoBackend.Application.Handler.Documents;
using IlpRepoBackend.Domain.Interfaces;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Linq.Expressions;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.Documents
{
    public class CreateDocumentTypeHandlerTests
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<ILogger<CreateDocumentTypeHandler>> _loggerMock;
        private readonly CreateDocumentTypeHandler _handler;

        public CreateDocumentTypeHandlerTests()
        {
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _loggerMock = new Mock<ILogger<CreateDocumentTypeHandler>>();
            _handler = new CreateDocumentTypeHandler(
                _documentRepositoryMock.Object,
                _fileStorageServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommandWithoutFile_CreatesDocumentType()
        {
            // Arrange
            var command = new CreateDocumentTypeCommand("Project Proposal", null, "pdf");

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync((DocumentEntity)null);
            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentEntity>()))
                .ReturnsAsync((DocumentEntity doc) =>
                {
                    doc.Id = 1;
                    return doc;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(1);
            result.Data.Name.ShouldBe("Project Proposal");
            result.Data.FileType.ShouldBe("pdf");
            result.Data.Link.ShouldBeNull();
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommandWithFile_CreatesDocumentTypeWithFileUrl()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("template.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var command = new CreateDocumentTypeCommand("Project Proposal", fileMock.Object, "pdf");

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync((DocumentEntity)null);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://storage.example.com/documents/template.pdf");
            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentEntity>()))
                .ReturnsAsync((DocumentEntity doc) =>
                {
                    doc.Id = 1;
                    return doc;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(1);
            result.Data.Name.ShouldBe("Project Proposal");
            result.Data.Link.ShouldBe("https://storage.example.com/documents/template.pdf");
            _fileStorageServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), "template.pdf", "application/pdf"), Times.Once);
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateDocumentTypeCommand("Existing Document", null);

            var existingDoc = new DocumentEntity
            {
                Id = 1,
                Name = "Existing Document"
            };

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync(existingDoc);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);
            fileMock.Setup(f => f.FileName).Returns("empty.pdf");

            var command = new CreateDocumentTypeCommand("Project Proposal", fileMock.Object);

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync((DocumentEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("empty");
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentEntity>()), Times.Never);
        }

        [Theory]
        [InlineData("Project Proposal", "pdf")]
        [InlineData("Status Report", "docx")]
        [InlineData("Sprint Review", "pptx")]
        public async Task Handle_DifferentDocumentTypes_CreatesSuccessfully(string name, string fileType)
        {
            // Arrange
            var command = new CreateDocumentTypeCommand(name, null, fileType);

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync((DocumentEntity)null);
            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentEntity>()))
                .ReturnsAsync((DocumentEntity doc) =>
                {
                    doc.Id = 1;
                    return doc;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Name.ShouldBe(name);
            result.Data.FileType.ShouldBe(fileType);
        }

        [Fact]
        public async Task Handle_FileUploadFails_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("template.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var command = new CreateDocumentTypeCommand("Project Proposal", fileMock.Object);

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync((DocumentEntity)null);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Storage service unavailable"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to create document type");
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var command = new CreateDocumentTypeCommand("Project Proposal");

            _documentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<DocumentEntity, bool>>>()))
                .ReturnsAsync((DocumentEntity)null);
            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentEntity>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to create document type");
        }
    }
}
