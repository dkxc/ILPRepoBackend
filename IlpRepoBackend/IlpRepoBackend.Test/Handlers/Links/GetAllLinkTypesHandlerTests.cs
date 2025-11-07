using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Handler.Links;
using IlpRepoBackend.Application.Query.Links;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Links
{
    public class GetAllLinkTypesHandlerTests
    {
        private readonly Mock<ILinkRepository> _linkRepositoryMock;
        private readonly GetAllLinkTypesHandler _handler;

        public GetAllLinkTypesHandlerTests()
        {
            _linkRepositoryMock = new Mock<ILinkRepository>();
            _handler = new GetAllLinkTypesHandler(_linkRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAllLinkTypes_Successfully()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" },
                new Link { Id = 2, Name = "Figma" },
                new Link { Id = 3, Name = "Jira" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(3);
            result.Message.ShouldBe("Link types retrieved successfully");

            result.Data[0].Id.ShouldBe(1);
            result.Data[0].Name.ShouldBe("GitHub");
            result.Data[1].Id.ShouldBe(2);
            result.Data[1].Name.ShouldBe("Figma");
            result.Data[2].Id.ShouldBe(3);
            result.Data[2].Name.ShouldBe("Jira");
        }

        [Fact]
        public async Task Handle_NoLinkTypes_ReturnsEmptyList()
        {
            // Arrange
            var linkTypes = new List<Link>();

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
            result.Data.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_SingleLinkType_ReturnsSingle()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].Id.ShouldBe(1);
            result.Data[0].Name.ShouldBe("GitHub");
        }

        [Fact]
        public async Task Handle_LinkTypeWithNullName_ReturnsEmptyString()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = null }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Name.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Handle_ManyLinkTypes_ReturnsAll()
        {
            // Arrange
            var linkTypes = Enumerable.Range(1, 20)
                .Select(i => new Link { Id = i, Name = $"Link Type {i}" })
                .ToList();

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(20);
            result.Data.All(lt => !string.IsNullOrEmpty(lt.Name)).ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_LinkTypesWithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = "Azure DevOps" },
                new Link { Id = 2, Name = "Google Drive" },
                new Link { Id = 3, Name = "Stack Overflow" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            result.Data.ShouldContain(lt => lt.Name == "Azure DevOps");
            result.Data.ShouldContain(lt => lt.Name == "Google Drive");
        }

        [Fact]
        public async Task Handle_LinkTypesInOrder_MaintainsOrder()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 5, Name = "E-Link" },
                new Link { Id = 3, Name = "C-Link" },
                new Link { Id = 1, Name = "A-Link" },
                new Link { Id = 4, Name = "D-Link" },
                new Link { Id = 2, Name = "B-Link" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(5);
            result.Data[0].Id.ShouldBe(5);
            result.Data[1].Id.ShouldBe(3);
            result.Data[2].Id.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_MapsToLinkTypeDto_Correctly()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link 
                { 
                    Id = 1, 
                    Name = "GitHub",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data[0].ShouldBeOfType<LinkTypeDto>();
            result.Data[0].Id.ShouldBe(1);
            result.Data[0].Name.ShouldBe("GitHub");
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            _linkRepositoryMock.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database connection error"));

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error retrieving link types");
            result.Message.ShouldContain("Database connection error");
            result.Data.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            var linkTypes = new List<Link>();
            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _linkRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_CommonLinkTypes_ReturnsCorrectly()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" },
                new Link { Id = 2, Name = "GitLab" },
                new Link { Id = 3, Name = "Bitbucket" },
                new Link { Id = 4, Name = "Figma" },
                new Link { Id = 5, Name = "Jira" },
                new Link { Id = 6, Name = "Confluence" },
                new Link { Id = 7, Name = "Trello" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(7);
            result.Data.ShouldContain(lt => lt.Name == "GitHub");
            result.Data.ShouldContain(lt => lt.Name == "Figma");
            result.Data.ShouldContain(lt => lt.Name == "Jira");
        }

        [Fact]
        public async Task Handle_LinkTypesWithLongNames_HandlesCorrectly()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = "Very Long Link Type Name For Testing Maximum Length Handling" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Name.Length.ShouldBeGreaterThan(50);
        }

        [Fact]
        public async Task Handle_MixedCaseLinkTypeNames_PreservesCase()
        {
            // Arrange
            var linkTypes = new List<Link>
            {
                new Link { Id = 1, Name = "GitHub" },
                new Link { Id = 2, Name = "GITLAB" },
                new Link { Id = 3, Name = "bitbucket" }
            };

            _linkRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(linkTypes);

            var query = new GetAllLinkTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data[0].Name.ShouldBe("GitHub");
            result.Data[1].Name.ShouldBe("GITLAB");
            result.Data[2].Name.ShouldBe("bitbucket");
        }
    }
}
