using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.AdminDashboard;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.AdminDashboard
{
    public class GetAdminDashboardSummaryQueryHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly GetAdminDashboardSummaryQueryHandler _handler;

        public GetAdminDashboardSummaryQueryHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _handler = new GetAdminDashboardSummaryQueryHandler(
                _batchRepositoryMock.Object,
                _projectRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSummaryWithCounts()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 1, BatchName = "Batch 1" },
                new Batch { Id = 2, BatchName = "Batch 2" },
                new Batch { Id = 3, BatchName = "Batch 3" }
            };

            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" },
                new Project { Id = 2, ProjectName = "Project 2" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetAdminDashboardSummaryQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.TotalBatches.ShouldBe(3);
            result.TotalProjects.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_NoBatchesOrProjects_ReturnsZero()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Batch>());
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Project>());

            var query = new GetAdminDashboardSummaryQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.TotalBatches.ShouldBe(0);
            result.TotalProjects.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_OnlyBatches_ReturnsCorrectCounts()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 1, BatchName = "Batch 1" },
                new Batch { Id = 2, BatchName = "Batch 2" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Project>());

            var query = new GetAdminDashboardSummaryQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalBatches.ShouldBe(2);
            result.TotalProjects.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_OnlyProjects_ReturnsCorrectCounts()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, ProjectName = "Project 1" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Batch>());
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetAdminDashboardSummaryQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalBatches.ShouldBe(0);
            result.TotalProjects.ShouldBe(1);
        }

        [Fact]
        public async Task Handle_LargeNumberOfRecords_ReturnsCorrectCount()
        {
            // Arrange
            var batches = Enumerable.Range(1, 100).Select(i => new Batch { Id = i, BatchName = $"Batch {i}" }).ToList();
            var projects = Enumerable.Range(1, 50).Select(i => new Project { Id = i, ProjectName = $"Project {i}" }).ToList();

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);
            _projectRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projects);

            var query = new GetAdminDashboardSummaryQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalBatches.ShouldBe(100);
            result.TotalProjects.ShouldBe(50);
        }
    }
}
