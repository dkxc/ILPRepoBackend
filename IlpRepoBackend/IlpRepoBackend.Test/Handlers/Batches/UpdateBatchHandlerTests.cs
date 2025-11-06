using AutoMapper;
using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Batchs;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Batches
{
    public class UpdateBatchHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly UpdateBatchHandler _handler;

        public UpdateBatchHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _handler = new UpdateBatchHandler(_mapperMock.Object, _batchRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesBatch()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Updated Batch",
                BatchTypeId = 2,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddMonths(3),
                Phases = new List<PhaseDto>()
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                BatchTypeId = 1,
                Status = BatchStatus.NotStarted,
                Phases = new List<Phase>()
            };

            var updatedBatch = new Batch
            {
                Id = 1,
                BatchName = command.BatchName,
                BatchTypeId = command.BatchTypeId,
                Status = BatchStatus.NotStarted,
                StartDate = command.StartDate,
                EndDate = command.EndDate
            };

            var batchDto = new BatchDto
            {
                Id = 1,
                BatchName = command.BatchName,
                Status = BatchStatus.NotStarted
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .ReturnsAsync(updatedBatch);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(updatedBatch.Id))
                .ReturnsAsync(updatedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(updatedBatch))
                .Returns(batchDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.BatchName.ShouldBe(command.BatchName);
            _batchRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Batch>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 999,
                BatchName = "Test Batch",
                BatchTypeId = 1
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync((Batch?)null);

            // Act & Assert
            var exception = await Should.ThrowAsync<KeyNotFoundException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("999");
            exception.Message.ShouldContain("not found");
            _batchRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Batch>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(
                async () => await _handler.Handle(null!, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_InvalidId_ThrowsArgumentNullException()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 0,
                BatchName = "Test Batch"
            };

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_FutureStartDate_SetsStatusToNotStarted()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Future Batch",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddMonths(3)
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                Phases = new List<Phase>()
            };

            var updatedBatch = new Batch
            {
                Id = 1,
                BatchName = command.BatchName,
                Status = BatchStatus.NotStarted,
                Phases = new List<Phase>()
            };

            _batchRepositoryMock.SetupSequence(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch)
                .ReturnsAsync(updatedBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .ReturnsAsync(updatedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns((Batch b) => new BatchDto { Status = b.Status });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Status.ShouldBe(BatchStatus.NotStarted);
        }

        [Fact]
        public async Task Handle_OngoingDates_SetsStatusToOngoing()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Ongoing Batch",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                Phases = new List<Phase>()
            };

            var updatedBatch = new Batch
            {
                Id = 1,
                BatchName = command.BatchName,
                Status = BatchStatus.Ongoing,
                Phases = new List<Phase>()
            };

            _batchRepositoryMock.SetupSequence(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch)
                .ReturnsAsync(updatedBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .ReturnsAsync(updatedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns((Batch b) => new BatchDto { Status = b.Status });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Status.ShouldBe(BatchStatus.Ongoing);
        }

        [Fact]
        public async Task Handle_PastEndDate_SetsStatusToCompleted()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Completed Batch",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddMonths(-3),
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                Phases = new List<Phase>()
            };

            var updatedBatch = new Batch
            {
                Id = 1,
                BatchName = command.BatchName,
                Status = BatchStatus.Completed,
                Phases = new List<Phase>()
            };

            _batchRepositoryMock.SetupSequence(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch)
                .ReturnsAsync(updatedBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .ReturnsAsync(updatedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns((Batch b) => new BatchDto { Status = b.Status });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Status.ShouldBe(BatchStatus.Completed);
        }

        [Fact]
        public async Task Handle_WithPhases_UpdatesPhases()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Batch with Phases",
                BatchTypeId = 1,
                Phases = new List<PhaseDto>
                {
                    new PhaseDto { PhaseType = "Foundation", PhaseTypeId = 1 },
                    new PhaseDto { PhaseType = "Advanced", PhaseTypeId = 2 }
                }
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                Phases = new List<Phase>
                {
                    new Phase { Id = 10, PhaseType = "Old Phase" }
                }
            };

            Batch? capturedBatch = null;

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .Callback<Batch>(b => capturedBatch = b)
                .ReturnsAsync((Batch b) => b);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => capturedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedBatch.ShouldNotBeNull();
            capturedBatch.Phases.ShouldNotBeNull();
            capturedBatch.Phases.Count.ShouldBe(2);
            capturedBatch.Phases.First().PhaseType.ShouldBe("Foundation");
            capturedBatch.Phases.Last().PhaseType.ShouldBe("Advanced");
        }

        [Fact]
        public async Task Handle_ClearsExistingPhases_WhenUpdatingWithNewPhases()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Updated Batch",
                BatchTypeId = 1,
                Phases = new List<PhaseDto>
                {
                    new PhaseDto { PhaseType = "New Phase", PhaseTypeId = 1 }
                }
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                Phases = new List<Phase>
                {
                    new Phase { Id = 1, PhaseType = "Old Phase 1" },
                    new Phase { Id = 2, PhaseType = "Old Phase 2" }
                }
            };

            Batch? capturedBatch = null;

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .Callback<Batch>(b => capturedBatch = b)
                .ReturnsAsync((Batch b) => b);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => capturedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedBatch.ShouldNotBeNull();
            capturedBatch.Phases.Count.ShouldBe(1);
            capturedBatch.Phases.First().PhaseType.ShouldBe("New Phase");
        }

        [Fact]
        public async Task Handle_UpdatesTimestamp()
        {
            // Arrange
            var command = new UpdateBatchCommand
            {
                Id = 1,
                BatchName = "Test Batch",
                BatchTypeId = 1
            };

            var existingBatch = new Batch
            {
                Id = 1,
                BatchName = "Old Batch",
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Phases = new List<Phase>()
            };

            var oldTimestamp = existingBatch.UpdatedAt;
            Batch? capturedBatch = null;

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingBatch);
            _batchRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Batch>()))
                .Callback<Batch>(b => capturedBatch = b)
                .ReturnsAsync((Batch b) => b);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => capturedBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedBatch.ShouldNotBeNull();
            capturedBatch.UpdatedAt.ShouldBeGreaterThan(oldTimestamp);
        }
    }
}
