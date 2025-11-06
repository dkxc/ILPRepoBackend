using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Batchs;
using IlpRepoBackend.Application.Query.Batchs;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Batches
{
    public class GetBatchByIdQueryHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly GetBatchByIdQueryHandler _handler;

        public GetBatchByIdQueryHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _handler = new GetBatchByIdQueryHandler(_mapperMock.Object, _batchRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchId_ReturnsBatch()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Status = BatchStatus.Ongoing,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3)
            };

            var batchDto = new BatchDto
            {
                Id = batchId,
                BatchName = "Test Batch",
                Status = BatchStatus.Ongoing
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<BatchDto>(batch))
                .Returns(batchDto);

            var query = new GetBatchByIdQuery { Id = batchId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(batchId);
            result.Data.BatchName.ShouldBe("Test Batch");
            result.Data.Status.ShouldBe(BatchStatus.Ongoing);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var batchId = 999;
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync((Batch?)null);

            var query = new GetBatchByIdQuery { Id = batchId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("999");
            result.Message.ShouldContain("not found");
            result.Data.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_InvalidBatchId_ReturnsFailure()
        {
            // Arrange
            var query = new GetBatchByIdQuery { Id = 0 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid");
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NegativeBatchId_ReturnsFailure()
        {
            // Arrange
            var query = new GetBatchByIdQuery { Id = -1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid");
        }

        [Fact]
        public async Task Handle_NullQuery_ReturnsFailure()
        {
            // Act
            var result = await _handler.Handle(null!, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task Handle_DifferentBatchIds_CallsRepositoryWithCorrectId(int batchId)
        {
            // Arrange
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto { Id = batchId });

            var query = new GetBatchByIdQuery { Id = batchId };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(batchId), Times.Once);
        }

        [Theory]
        [InlineData(BatchStatus.NotStarted)]
        [InlineData(BatchStatus.Ongoing)]
        [InlineData(BatchStatus.Completed)]
        public async Task Handle_BatchesWithDifferentStatuses_ReturnsCorrectly(BatchStatus status)
        {
            // Arrange
            var batch = new Batch
            {
                Id = 1,
                BatchName = "Test Batch",
                Status = status
            };

            var batchDto = new BatchDto
            {
                Id = 1,
                Status = status
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<BatchDto>(batch))
                .Returns(batchDto);

            var query = new GetBatchByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Status.ShouldBe(status);
        }

        [Fact]
        public async Task Handle_BatchWithPhases_ReturnsComplete()
        {
            // Arrange
            var batch = new Batch
            {
                Id = 1,
                BatchName = "Batch with Phases",
                Phases = new List<Phase>
                {
                    new Phase { Id = 1, PhaseType = "Foundation" },
                    new Phase { Id = 2, PhaseType = "Advanced" }
                }
            };

            var batchDto = new BatchDto
            {
                Id = 1,
                BatchName = "Batch with Phases",
                Phases = new List<PhaseDto>
                {
                    new PhaseDto { PhaseType = "Foundation" },
                    new PhaseDto { PhaseType = "Advanced" }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<BatchDto>(batch))
                .Returns(batchDto);

            var query = new GetBatchByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Phases.ShouldNotBeNull();
            result.Data.Phases.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_MapperThrowsException_HandlesGracefully()
        {
            // Arrange
            var batch = new Batch { Id = 1, BatchName = "Test" };
            
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<BatchDto>(batch))
                .Throws(new Exception("Mapping error"));

            var query = new GetBatchByIdQuery { Id = 1 };

            // Act & Assert
            await Should.ThrowAsync<Exception>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );
        }
    }
}
