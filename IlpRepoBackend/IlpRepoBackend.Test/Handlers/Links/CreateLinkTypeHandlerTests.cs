using IlpRepoBackend.Application.Command.Links;
using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Handler.Links;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Links
{
    public class CreateLinkTypeHandlerTests
    {
        private readonly Mock<ILinkRepository> _linkRepositoryMock;
        private readonly CreateLinkTypeHandler _handler;

        public CreateLinkTypeHandlerTests()
        {
            _linkRepositoryMock = new Mock<ILinkRepository>();
            _handler = new CreateLinkTypeHandler(_linkRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidLinkTypeName_CreatesSuccessfully()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("GitHub");
            var existingLinks = new List<Link>();
            var createdLink = new Link
            {
                Id = 1,
                Name = "GitHub",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>())).ReturnsAsync(createdLink);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(1);
            result.Data.Name.ShouldBe("GitHub");
            result.Message.ShouldBe("Link type created successfully");

            _linkRepositoryMock.Verify(x => x.AddAsync(It.Is<Link>(l => 
                l.Name == "GitHub" && 
                l.CreatedAt != default && 
                l.UpdatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Link type name cannot be empty");

            _linkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand(null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Link type name cannot be empty");
        }

        [Fact]
        public async Task Handle_WhitespaceName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("   ");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Link type name cannot be empty");
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("GitHub");
            var existingLinks = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Link type 'GitHub' already exists");

            _linkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateNameCaseInsensitive_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("github");
            var existingLinks = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Link type 'github' already exists");
        }

        [Fact]
        public async Task Handle_DuplicateNameWithWhitespace_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("  GitHub  ");
            var existingLinks = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
        }

        [Fact]
        public async Task Handle_TrimsWhitespace_BeforeCreating()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("  Figma  ");
            var existingLinks = new List<Link>();
            var createdLink = new Link
            {
                Id = 1,
                Name = "Figma",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>())).ReturnsAsync(createdLink);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Name.ShouldBe("Figma");

            _linkRepositoryMock.Verify(x => x.AddAsync(It.Is<Link>(l => l.Name == "Figma")), Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleLinkTypes_CreatesSuccessfully()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("Jira");
            var existingLinks = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" },
                new Link { Id = 2, Name = "Figma" }
            };
            var createdLink = new Link
            {
                Id = 3,
                Name = "Jira",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>())).ReturnsAsync(createdLink);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Id.ShouldBe(3);
            result.Data.Name.ShouldBe("Jira");
        }

        [Fact]
        public async Task Handle_LongName_CreatesSuccessfully()
        {
            // Arrange
            var longName = "Very Long Link Type Name For Testing Purposes";
            var command = new CreateLinkTypeCommand(longName);
            var existingLinks = new List<Link>();
            var createdLink = new Link
            {
                Id = 1,
                Name = longName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>())).ReturnsAsync(createdLink);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Name.ShouldBe(longName);
        }

        [Fact]
        public async Task Handle_SpecialCharactersInName_CreatesSuccessfully()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("Azure DevOps");
            var existingLinks = new List<Link>();
            var createdLink = new Link
            {
                Id = 1,
                Name = "Azure DevOps",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>())).ReturnsAsync(createdLink);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Name.ShouldBe("Azure DevOps");
        }

        [Fact]
        public async Task Handle_SetsCreatedAndUpdatedAtTimestamps()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("GitLab");
            var existingLinks = new List<Link>();
            
            Link capturedLink = null;
            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>()))
                .Callback<Link>(l => capturedLink = l)
                .ReturnsAsync((Link l) => new Link
                {
                    Id = 1,
                    Name = l.Name,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedLink.ShouldNotBeNull();
            capturedLink.CreatedAt.ShouldNotBe(default);
            capturedLink.UpdatedAt.ShouldNotBe(default);
            capturedLink.CreatedAt.ShouldBe(capturedLink.UpdatedAt, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("GitHub");
            
            _linkRepositoryMock.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error creating link type");
            result.Message.ShouldContain("Database error");
        }

        [Fact]
        public async Task Handle_CreatedLinkTypeDto_ContainsAllProperties()
        {
            // Arrange
            var command = new CreateLinkTypeCommand("Trello");
            var existingLinks = new List<Link>();
            var now = DateTime.UtcNow;
            var createdLink = new Link
            {
                Id = 5,
                Name = "Trello",
                CreatedAt = now,
                UpdatedAt = now
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(existingLinks);
            _linkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Link>())).ReturnsAsync(createdLink);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Data.Id.ShouldBe(5);
            result.Data.Name.ShouldBe("Trello");
            result.Data.CreatedAt.ShouldBe(now);
            result.Data.UpdatedAt.ShouldBe(now);
        }
    }
}
