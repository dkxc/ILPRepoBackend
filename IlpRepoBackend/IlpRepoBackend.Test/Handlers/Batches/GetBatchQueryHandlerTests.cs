using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Batchs;
using IlpRepoBackend.Application.Query.Batchs;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using Moq;
using Shouldly;
using Xunit;
using IlpRepoBackend.Domain.Persistence;

namespace IlpRepoBackend.Test.Handlers.Batches
{
    public class GetBatchQueryHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly GetBatchQueryHadler _handler;

        public GetBatchQueryHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _handler = new GetBatchQueryHadler(_mapperMock.Object, _batchRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_BatchesExist_ReturnsAllBatches()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 1, BatchName = "Batch 1", Status = BatchStatus.Ongoing },
                new Batch { Id = 2, BatchName = "Batch 2", Status = BatchStatus.NotStarted }
            };

            var batchDtos = new List<BatchDto>
            {
                new BatchDto { Id = 1, BatchName = "Batch 1", Status = BatchStatus.Ongoing },
                new BatchDto { Id = 2, BatchName = "Batch 2", Status = BatchStatus.NotStarted }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(batches);
            _mapperMock.Setup(x => x.Map<List<BatchDto>>(batches))
                .Returns(batchDtos);

            var query = new GetBatchsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].BatchName.ShouldBe("Batch 1");
            result.Data[1].BatchName.ShouldBe("Batch 2");
        }

        [Fact]
        public async Task Handle_NoBatches_ReturnsEmptyList()
        {
            // Arrange
            var batches = new List<Batch>();
            var batchDtos = new List<BatchDto>();

            _batchRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(batches);
            _mapperMock.Setup(x => x.Map<List<BatchDto>>(batches))
                .Returns(batchDtos);

            var query = new GetBatchsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_BatchesWithDifferentStatuses_ReturnsAllBatches()
        {
            // Arrange
            var batches = new List<Batch>
            {
                new Batch { Id = 1, BatchName = "Batch 1", Status = BatchStatus.NotStarted },
                new Batch { Id = 2, BatchName = "Batch 2", Status = BatchStatus.Ongoing },
                new Batch { Id = 3, BatchName = "Batch 3", Status = BatchStatus.Completed }
            };

            var batchDtos = new List<BatchDto>
            {
                new BatchDto { Id = 1, Status = BatchStatus.NotStarted },
                new BatchDto { Id = 2, Status = BatchStatus.Ongoing },
                new BatchDto { Id = 3, Status = BatchStatus.Completed }
            };

            _batchRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(batches);
            _mapperMock.Setup(x => x.Map<List<BatchDto>>(batches))
                .Returns(batchDtos);

            var query = new GetBatchsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Count.ShouldBe(3);
            result.Data.ShouldContain(b => b.Status == BatchStatus.NotStarted);
            result.Data.ShouldContain(b => b.Status == BatchStatus.Ongoing);
            result.Data.ShouldContain(b => b.Status == BatchStatus.Completed);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Batch>());
            _mapperMock.Setup(x => x.Map<List<BatchDto>>(It.IsAny<List<Batch>>()))
                .Returns(new List<BatchDto>());

            var query = new GetBatchsQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }
    }
}
