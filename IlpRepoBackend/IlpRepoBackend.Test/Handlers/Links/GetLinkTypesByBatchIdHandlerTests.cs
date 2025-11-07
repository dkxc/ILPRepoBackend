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
    public class GetLinkTypesByBatchIdHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectLinkRepository> _projectLinkRepositoryMock;
        private readonly Mock<ILinkRepository> _linkRepositoryMock;
        private readonly GetLinkTypesByBatchIdHandler _handler;

        public GetLinkTypesByBatchIdHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectLinkRepositoryMock = new Mock<IProjectLinkRepository>();
            _linkRepositoryMock = new Mock<ILinkRepository>();
            
            _handler = new GetLinkTypesByBatchIdHandler(
                _batchRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _projectLinkRepositoryMock.Object,
                _linkRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchWithLinks_ReturnsStatistics()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);

            var batch = new Batch { Id = batchId, BatchName = "ILP Batch 2024" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" },
                new Project { Id = 3, ProjectName = "Project 3" }
            };
            var linkType = new Link { Id = 1, Name = "GitHub" };

            // Project 1 and 2 have submitted GitHub link, Project 3 hasn't
            var project1Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1, LinkUrl = "https://github.com/project1" }
            };
            var project2Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 2, ProjectId = 2, LinkId = 1, LinkUrl = "https://github.com/project2" }
            };
            var project3Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 3, ProjectId = 3, LinkId = 1, LinkUrl = null } // Not submitted
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(project1Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(2)).ReturnsAsync(project2Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(3)).ReturnsAsync(project3Links);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            
            var linkStats = result.Data[0];
            linkStats.LinkTypeId.ShouldBe(1);
            linkStats.LinkTypeName.ShouldBe("GitHub");
            linkStats.TotalProjects.ShouldBe(3);
            linkStats.SubmittedProjects.ShouldBe(2);
            linkStats.PendingProjects.ShouldBe(1);
            linkStats.CompletionPercentage.ShouldBe(66.7, 0.1);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var query = new GetLinkTypesByBatchIdQuery(999);

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Batch)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Batch with ID 999 not found");
        }

        [Fact]
        public async Task Handle_NoProjectsInBatch_ReturnsEmptyListWithSuccess()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Empty Batch" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Project>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeEmpty();
            result.Message.ShouldContain("No projects found");
        }

        [Fact]
        public async Task Handle_ProjectsWithoutLinks_ReturnsEmptyList()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<ProjectLink>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_MultipleLinkTypes_ReturnsAllWithStatistics()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" }
            };

            var gitHubLink = new Link { Id = 1, Name = "GitHub" };
            var figmaLink = new Link { Id = 2, Name = "Figma" };

            var project1Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1, LinkUrl = "https://github.com/p1" },
                new ProjectLink { Id = 2, ProjectId = 1, LinkId = 2, LinkUrl = "https://figma.com/p1" }
            };
            var project2Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 3, ProjectId = 2, LinkId = 1, LinkUrl = null }, // GitHub not submitted
                new ProjectLink { Id = 4, ProjectId = 2, LinkId = 2, LinkUrl = "https://figma.com/p2" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(project1Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(2)).ReturnsAsync(project2Links);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(gitHubLink);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(figmaLink);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);

            var githubStats = result.Data.First(d => d.LinkTypeId == 1);
            githubStats.LinkTypeName.ShouldBe("GitHub");
            githubStats.SubmittedProjects.ShouldBe(1);
            githubStats.PendingProjects.ShouldBe(1);
            githubStats.CompletionPercentage.ShouldBe(50.0);

            var figmaStats = result.Data.First(d => d.LinkTypeId == 2);
            figmaStats.LinkTypeName.ShouldBe("Figma");
            figmaStats.SubmittedProjects.ShouldBe(2);
            figmaStats.PendingProjects.ShouldBe(0);
            figmaStats.CompletionPercentage.ShouldBe(100.0);
        }

        [Fact]
        public async Task Handle_AllProjectsSubmitted_Returns100PercentCompletion()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" }
            };
            var linkType = new Link { Id = 1, Name = "Jira" };

            var project1Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1, LinkUrl = "https://jira.com/p1" }
            };
            var project2Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 2, ProjectId = 2, LinkId = 1, LinkUrl = "https://jira.com/p2" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(project1Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(2)).ReturnsAsync(project2Links);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data[0].CompletionPercentage.ShouldBe(100.0);
            result.Data[0].SubmittedProjects.ShouldBe(2);
            result.Data[0].PendingProjects.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_NoProjectsSubmitted_ReturnsZeroPercentCompletion()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" }
            };
            var linkType = new Link { Id = 1, Name = "Trello" };

            var project1Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1, LinkUrl = null }
            };
            var project2Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 2, ProjectId = 2, LinkId = 1, LinkUrl = "" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(project1Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(2)).ReturnsAsync(project2Links);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data[0].CompletionPercentage.ShouldBe(0.0);
            result.Data[0].SubmittedProjects.ShouldBe(0);
            result.Data[0].PendingProjects.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_LinkTypeWithoutName_ReturnsUnknown()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };
            var linkType = new Link { Id = 1, Name = null };

            var projectLinks = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1, LinkUrl = "https://test.com" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(projectLinks);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data[0].LinkTypeName.ShouldBe("Unknown");
        }

        [Fact]
        public async Task Handle_LinkTypeNotFound_SkipsIt()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            var projectLinks = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 999, LinkUrl = "https://test.com" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(projectLinks);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Link)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_ProjectLinksWithNullLinkId_AreIgnored()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            var projectLinks = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = null, LinkUrl = "https://test.com" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(projectLinks);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_ManyProjects_CalculatesStatisticsCorrectly()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Large Batch" };
            var projects = Enumerable.Range(1, 100)
                .Select(i => new Project { Id = i, ProjectName = $"Project {i}" })
                .ToList();
            var linkType = new Link { Id = 1, Name = "GitHub" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            
            // 75 projects submitted, 25 didn't
            foreach (var project in projects)
            {
                var hasUrl = project.Id <= 75;
                _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(project.Id))
                    .ReturnsAsync(new List<ProjectLink>
                    {
                        new ProjectLink 
                        { 
                            Id = project.Id, 
                            ProjectId = project.Id, 
                            LinkId = 1, 
                            LinkUrl = hasUrl ? $"https://github.com/project{project.Id}" : null 
                        }
                    });
            }
            
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].TotalProjects.ShouldBe(100);
            result.Data[0].SubmittedProjects.ShouldBe(75);
            result.Data[0].PendingProjects.ShouldBe(25);
            result.Data[0].CompletionPercentage.ShouldBe(75.0);
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var query = new GetLinkTypesByBatchIdQuery(1);

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error retrieving link types for batch");
            result.Message.ShouldContain("Database error");
        }

        [Fact]
        public async Task Handle_SuccessMessage_ContainsBatchId()
        {
            // Arrange
            var batchId = 42;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1))
                .ReturnsAsync(new List<ProjectLink>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Message.ShouldContain("42");
        }

        [Fact]
        public async Task Handle_CompletionPercentage_RoundsToOneDecimal()
        {
            // Arrange
            var batchId = 1;
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" },
                new Project { Id = 3, ProjectName = "Project 3" }
            };
            var linkType = new Link { Id = 1, Name = "GitLab" };

            // 2 out of 3 submitted = 66.666...%
            var project1Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 1, ProjectId = 1, LinkId = 1, LinkUrl = "url1" }
            };
            var project2Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 2, ProjectId = 2, LinkId = 1, LinkUrl = "url2" }
            };
            var project3Links = new List<ProjectLink>
            {
                new ProjectLink { Id = 3, ProjectId = 3, LinkId = 1, LinkUrl = null }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _projectRepositoryMock.Setup(x => x.GetProjectsByBatchIdAsync(batchId)).ReturnsAsync(projects);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(1)).ReturnsAsync(project1Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(2)).ReturnsAsync(project2Links);
            _projectLinkRepositoryMock.Setup(x => x.GetByProjectIdAsync(3)).ReturnsAsync(project3Links);
            _linkRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(linkType);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data[0].CompletionPercentage.ShouldBe(66.7, 0.1);
        }
    }
}
