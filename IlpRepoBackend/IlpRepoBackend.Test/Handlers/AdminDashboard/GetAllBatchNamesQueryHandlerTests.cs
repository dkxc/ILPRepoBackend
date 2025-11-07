using AutoMapper;
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
    public class GetAllBatchNamesQueryHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAllBatchNamesQueryHandler _handler;

        public GetAllBatchNamesQueryHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetAllBatchNamesQueryHandler(
                _batchRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAllBatchNames()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 1, BatchName = "ILP Batch 1" },
                new Batch { Id = 2, BatchName = "ILP Batch 2" },
                new Batch { Id = 3, BatchName = "Graduate Batch 1" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);

            var query = new GetAllBatchNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result[0].BatchId.ShouldBe(1);
            result[0].BatchName.ShouldBe("ILP Batch 1");
            result[1].BatchId.ShouldBe(2);
            result[1].BatchName.ShouldBe("ILP Batch 2");
            result[2].BatchId.ShouldBe(3);
            result[2].BatchName.ShouldBe("Graduate Batch 1");
        }

        [Fact]
        public async Task Handle_NoBatches_ReturnsEmptyList()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Batch>());

            var query = new GetAllBatchNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_SingleBatch_ReturnsOne()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 5, BatchName = "Special Batch" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);

            var query = new GetAllBatchNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1);
            result[0].BatchId.ShouldBe(5);
            result[0].BatchName.ShouldBe("Special Batch");
        }

        [Fact]
        public async Task Handle_MapsCorrectly()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 10, BatchName = "Test Batch" },
                new Batch { Id = 20, BatchName = "Another Batch" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);

            var query = new GetAllBatchNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(2);
            result.All(b => b.BatchId > 0).ShouldBeTrue();
            result.All(b => !string.IsNullOrEmpty(b.BatchName)).ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_MultipleBatches_MaintainsOrder()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 3, BatchName = "Batch C" },
                new Batch { Id = 1, BatchName = "Batch A" },
                new Batch { Id = 2, BatchName = "Batch B" }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(batches);

            var query = new GetAllBatchNamesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(3);
            result[0].BatchId.ShouldBe(3);
            result[1].BatchId.ShouldBe(1);
            result[2].BatchId.ShouldBe(2);
        }
    }
}
