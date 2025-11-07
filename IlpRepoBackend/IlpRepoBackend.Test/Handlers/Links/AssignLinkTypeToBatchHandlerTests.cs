using IlpRepoBackend.Application.Command.Links;
using IlpRepoBackend.Application.Handler.Links;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Links
{
    public class AssignLinkTypeToBatchHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<ILinkRepository> _linkRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectLinkRepository> _projectLinkRepositoryMock;
        private readonly AssignLinkTypeToBatchHandler _handler;

        public AssignLinkTypeToBatchHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _linkRepositoryMock = new Mock<ILinkRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectLinkRepositoryMock = new Mock<IProjectLinkRepository>();
            
            _handler = new AssignLinkTypeToBatchHandler(
                _batchRepositoryMock.Object,
                _linkRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _projectLinkRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchAndLinkType_AssignsSuccessfully()
        {
            // Arrange
            var batchId = 1;
            var linkTypeId = 1;
            var command = new AssignLinkTypeToBatchCommand(batchId, linkTypeId);

            var batch = new Batch { Id = batchId, BatchName = "ILP Batch 2024" };
            var linkType = new Link { Id = linkTypeId, Name = "GitHub" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(linkTypeId)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProjectLink>());
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeTrue();
            result.Message.ShouldContain("GitHub");
            result.Message.ShouldContain("ILP Batch 2024");
            result.Message.ShouldContain("Created 2 new requirements");

            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectLink>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(999, 1);

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Batch)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Batch with ID 999 not found");

            _linkRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LinkTypeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 999);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Link)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Link type with ID 999 not found");

            _projectRepositoryMock.Verify(x => x.GetProjectsByBatchIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoProjectsInBatch_ReturnsFailure()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Empty Batch" };
            var linkType = new Link { Id = 1, Name = "GitHub" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1))
                .ReturnsAsync(new List<Project>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("No projects found for batch 1");

            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectLink>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LinkTypeAlreadyAssignedToAllProjects_ReturnsSuccessWithZeroCreated()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var linkType = new Link { Id = 1, Name = "GitHub" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" }
            };
            var existingProjectLinks = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1 }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existingProjectLinks);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("Created 0 new requirements");
            result.Message.ShouldContain("2 already existed");

            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectLink>()), Times.Never);
        }

        [Fact]
        public async Task Handle_PartiallyAssigned_CreatesOnlyMissingLinks()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var linkType = new Link { Id = 1, Name = "GitHub" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" },
                new Project { Id = 3, ProjectName = "Project 3" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            
            // Project 1 already has the link type
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<ProjectLink>
                {
                    new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1 }
                });
            
            // Project 2 and 3 don't have it
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(2))
                .ReturnsAsync(new List<ProjectLink>());
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(3))
                .ReturnsAsync(new List<ProjectLink>());
            
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("Created 2 new requirements");
            result.Message.ShouldContain("1 already existed");

            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectLink>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_CreatesProjectLinkWithNullUrl()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var linkType = new Link { Id = 1, Name = "GitHub" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            ProjectLink capturedProjectLink = null;

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<ProjectLink>());
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .Callback<ProjectLink>(pl => capturedProjectLink = pl)
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedProjectLink.ShouldNotBeNull();
            capturedProjectLink.ProjectId.ShouldBe(1);
            capturedProjectLink.LinkId.ShouldBe(1);
            capturedProjectLink.LinkUrl.ShouldBeNull();
            capturedProjectLink.CreatedAt.ShouldNotBe(default);
            capturedProjectLink.UpdatedAt.ShouldNotBe(default);
        }

        [Fact]
        public async Task Handle_MultipleProjects_AssignsToAll()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Large Batch" };
            var linkType = new Link { Id = 1, Name = "Figma" };
            var projects = Enumerable.Range(1, 10)
                .Select(i => new Project { Id = i, ProjectName = $"Project {i}" })
                .ToList();

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProjectLink>());
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("Created 10 new requirements");

            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectLink>()), Times.Exactly(10));
        }

        [Fact]
        public async Task Handle_DifferentLinkTypes_AssignsCorrectLinkType()
        {
            // Arrange
            var jiraLinkTypeId = 2;
            var command = new AssignLinkTypeToBatchCommand(1, jiraLinkTypeId);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var linkType = new Link { Id = jiraLinkTypeId, Name = "Jira" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            // Project already has GitHub (linkId = 1), but not Jira (linkId = 2)
            var existingProjectLinks = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1 }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(jiraLinkTypeId)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(existingProjectLinks);
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldContain("Created 1 new requirements");

            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.Is<ProjectLink>(pl => 
                pl.LinkId == jiraLinkTypeId)), Times.Once);
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error assigning link type to batch");
            result.Message.ShouldContain("Database error");
        }

        [Fact]
        public async Task Handle_SetsTimestampsCorrectly()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var linkType = new Link { Id = 1, Name = "GitLab" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            ProjectLink capturedLink = null;

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<ProjectLink>());
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .Callback<ProjectLink>(pl => capturedLink = pl)
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedLink.ShouldNotBeNull();
            capturedLink.CreatedAt.ShouldNotBe(default);
            capturedLink.UpdatedAt.ShouldNotBe(default);
            capturedLink.CreatedAt.ShouldBe(capturedLink.UpdatedAt, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_CallsAllRepositoriesInCorrectOrder()
        {
            // Arrange
            var command = new AssignLinkTypeToBatchCommand(1, 1);
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var linkType = new Link { Id = 1, Name = "Trello" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(1)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<ProjectLink>());
            _projectLinkRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectLink>()))
                .ReturnsAsync((ProjectLink pl) => pl);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
            _linkRepositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
            _projectRepositoryMock.Verify(x => x.GetProjectsByBatchIdAsync(1), Times.Once);
            _projectLinkRepositoryMock.Verify(x => x.GetByProjectIdAsync(1), Times.Once);
            _projectLinkRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectLink>()), Times.Once);
        }
    }
}
