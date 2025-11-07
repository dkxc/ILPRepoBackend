using IlpRepoBackend.Application.Handler.Documents;
using IlpRepoBackend.Application.Query.Documents;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using DocumentEntity = IlpRepoBackend.Domain.Entities.Documents;

namespace IlpRepoBackend.Test.Handlers.Documents
{
    public class GetAllDocumentTypesHandlerTests
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<ILogger<GetAllDocumentTypesHandler>> _loggerMock;
        private readonly GetAllDocumentTypesHandler _handler;

        public GetAllDocumentTypesHandlerTests()
        {
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _loggerMock = new Mock<ILogger<GetAllDocumentTypesHandler>>();
            _handler = new GetAllDocumentTypesHandler(
                _documentRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_DocumentsExist_ReturnsAllDocumentTypes()
        {
            // Arrange
            var documents = new List<DocumentEntity>
            {
                new DocumentEntity
                {
                    Id = 1,
                    Name = "Project Proposal",
                    FileType = "pdf",
                    Link = "https://example.com/proposal.pdf",
                    UploadDate = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new DocumentEntity
                {
                    Id = 2,
                    Name = "Status Report",
                    FileType = "docx",
                    Link = "https://example.com/report.docx",
                    UploadDate = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _documentRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(documents);

            var query = new GetAllDocumentTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].Name.ShouldBe("Project Proposal");
            result.Data[1].Name.ShouldBe("Status Report");
            _documentRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_NoDocuments_ReturnsEmptyList()
        {
            // Arrange
            _documentRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<DocumentEntity>());

            var query = new GetAllDocumentTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_DocumentsReturned_AreSortedByName()
        {
            // Arrange
            var documents = new List<DocumentEntity>
            {
                new DocumentEntity { Id = 1, Name = "Zebra Document", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new DocumentEntity { Id = 2, Name = "Alpha Document", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new DocumentEntity { Id = 3, Name = "Beta Document", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            _documentRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(documents);

            var query = new GetAllDocumentTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Count.ShouldBe(3);
            result.Data[0].Name.ShouldBe("Alpha Document");
            result.Data[1].Name.ShouldBe("Beta Document");
            result.Data[2].Name.ShouldBe("Zebra Document");
        }

        [Fact]
        public async Task Handle_RepositoryException_ReturnsFailure()
        {
            // Arrange
            _documentRepositoryMock.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database connection failed"));

            var query = new GetAllDocumentTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to retrieve document types");
        }

        [Fact]
        public async Task Handle_DocumentsWithDifferentProperties_MapsCorrectly()
        {
            // Arrange
            var documents = new List<DocumentEntity>
            {
                new DocumentEntity
                {
                    Id = 1,
                    Name = "Document With Link",
                    FileType = "pdf",
                    Link = "https://example.com/doc.pdf",
                    UploadDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DocumentEntity
                {
                    Id = 2,
                    Name = "Document Without Link",
                    FileType = null,
                    Link = null,
                    UploadDate = DateTime.MinValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _documentRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(documents);

            var query = new GetAllDocumentTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            
            // Documents are sorted by name, so "Document With Link" comes first
            var docWithLink = result.Data.First(d => d.Name == "Document With Link");
            docWithLink.Link.ShouldNotBeNull();
            docWithLink.Link.ShouldBe("https://example.com/doc.pdf");
            
            var docWithoutLink = result.Data.First(d => d.Name == "Document Without Link");
            docWithoutLink.Link.ShouldBeNull();
        }
    }
}
