using IlpRepoBackend.Application.Command.DocumentSubmissions;
using IlpRepoBackend.Application.Handler.DocumentSubmissions;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Interfaces;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.DocumentSubmissions
{
    public class SubmitDocumentHandlerTests
    {
        private readonly Mock<IDocumentSubmissionRepository> _documentSubmissionRepositoryMock;
        private readonly Mock<IDocumentRequestRepository> _documentRequestRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<ILogger<SubmitDocumentHandler>> _loggerMock;
        private readonly SubmitDocumentHandler _handler;

        public SubmitDocumentHandlerTests()
        {
            _documentSubmissionRepositoryMock = new Mock<IDocumentSubmissionRepository>();
            _documentRequestRepositoryMock = new Mock<IDocumentRequestRepository>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _loggerMock = new Mock<ILogger<SubmitDocumentHandler>>();
            _handler = new SubmitDocumentHandler(
                _documentSubmissionRepositoryMock.Object,
                _documentRequestRepositoryMock.Object,
                _fileStorageServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSubmission_CreatesDocumentSubmission()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(2048);
            fileMock.Setup(f => f.FileName).Returns("project_proposal.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var documentRequest = new DocumentRequest
            {
                Id = 1,
                ProjectId = 1,
                DocumentId = 1,
                DueDate = DateTime.UtcNow.AddDays(7),
                RequestDate = DateTime.UtcNow.AddDays(-7),
                Document = new DocumentEntity { Id = 1, Name = "Project Proposal" },
                Project = new Project { Id = 1, ProjectName = "Test Project" }
            };

            var command = new SubmitDocumentCommand(1, fileMock.Object);

            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://storage.example.com/submissions/project_proposal.pdf");
            _documentSubmissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentSubmission>()))
                .ReturnsAsync((DocumentSubmission ds) =>
                {
                    ds.Id = 1;
                    return ds;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.SubmissionId.ShouldBe(1);
            result.Data.FileName.ShouldBe("project_proposal.pdf");
            result.Data.FileType.ShouldBe("PDF");
            result.Data.SubmissionLink.ShouldBe("https://storage.example.com/submissions/project_proposal.pdf");
            result.Data.DocumentTypeName.ShouldBe("Project Proposal");
            result.Data.ProjectName.ShouldBe("Test Project");
            result.Data.IsLateSubmission.ShouldBeFalse();
            _documentSubmissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentSubmission>()), Times.Once);
        }

        [Fact]
        public async Task Handle_LateSubmission_MarksAsLate()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(2048);
            fileMock.Setup(f => f.FileName).Returns("late_submission.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var documentRequest = new DocumentRequest
            {
                Id = 1,
                ProjectId = 1,
                DocumentId = 1,
                DueDate = DateTime.UtcNow.AddDays(-3), // Due date in the past
                RequestDate = DateTime.UtcNow.AddDays(-10),
                Document = new DocumentEntity { Id = 1, Name = "Project Proposal" },
                Project = new Project { Id = 1, ProjectName = "Test Project" }
            };

            var command = new SubmitDocumentCommand(1, fileMock.Object);

            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://storage.example.com/submissions/late_submission.pdf");
            _documentSubmissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentSubmission>()))
                .ReturnsAsync((DocumentSubmission ds) =>
                {
                    ds.Id = 1;
                    return ds;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.IsLateSubmission.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_DocumentRequestNotFound_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var command = new SubmitDocumentCommand(999, fileMock.Object);

            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(999))
                .ReturnsAsync((DocumentRequest)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _documentSubmissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentSubmission>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullFile_ReturnsFailure()
        {
            // Arrange
            var command = new SubmitDocumentCommand(1, null);

            var documentRequest = new DocumentRequest { Id = 1 };
            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("required");
        }

        [Fact]
        public async Task Handle_EmptyFile_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            var command = new SubmitDocumentCommand(1, fileMock.Object);

            var documentRequest = new DocumentRequest { Id = 1 };
            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("required");
        }

        [Theory]
        [InlineData("document.pdf", "PDF")]
        [InlineData("document.docx", "DOCX")]
        [InlineData("document.pptx", "PPTX")]
        [InlineData("document.xlsx", "XLSX")]
        public async Task Handle_DifferentFileTypes_ExtractsCorrectFileType(string fileName, string expectedFileType)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var documentRequest = new DocumentRequest
            {
                Id = 1,
                ProjectId = 1,
                DocumentId = 1,
                DueDate = DateTime.UtcNow.AddDays(7),
                Document = new DocumentEntity { Id = 1, Name = "Document" },
                Project = new Project { Id = 1, ProjectName = "Project" }
            };

            var command = new SubmitDocumentCommand(1, fileMock.Object);

            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync($"https://storage.example.com/{fileName}");
            _documentSubmissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentSubmission>()))
                .ReturnsAsync((DocumentSubmission ds) =>
                {
                    ds.Id = 1;
                    return ds;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.FileType.ShouldBe(expectedFileType);
        }

        [Fact]
        public async Task Handle_FileUploadFails_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("document.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var documentRequest = new DocumentRequest
            {
                Id = 1,
                ProjectId = 1,
                DocumentId = 1,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var command = new SubmitDocumentCommand(1, fileMock.Object);

            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Storage service error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to submit document");
            _documentSubmissionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentSubmission>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("document.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            var documentRequest = new DocumentRequest
            {
                Id = 1,
                ProjectId = 1,
                DocumentId = 1,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var command = new SubmitDocumentCommand(1, fileMock.Object);

            _documentRequestRepositoryMock.Setup(x => x.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(documentRequest);
            _fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://storage.example.com/document.pdf");
            _documentSubmissionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DocumentSubmission>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to submit document");
        }
    }
}
