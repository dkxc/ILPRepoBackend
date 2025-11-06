using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Handler.Batchs;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Batches
{
    public class DeleteBatchHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly DeleteBatchHandler _handler;

        public DeleteBatchHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _handler = new DeleteBatchHandler(_batchRepositoryMock.Object, _traineeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchWithNoRelations_DeletesSuccessfully()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>(),
                TrainingSchedules = new List<TrainingSchedule>()
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());
            _batchRepositoryMock.Setup(x => x.DeleteAsync(batchId))
                .ReturnsAsync(true);

            var command = new DeleteBatchCommand { Id = batchId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();
            _batchRepositoryMock.Verify(x => x.DeleteAsync(batchId), Times.Once);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var batchId = 999;
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync((Batch?)null);

            var command = new DeleteBatchCommand { Id = batchId };

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("not found");
            _batchRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchWithTrainees_ThrowsInvalidOperationException()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>(),
                TrainingSchedules = new List<TrainingSchedule>()
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId },
                new Trainee { Id = 2, BatchId = batchId }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(trainees);

            var command = new DeleteBatchCommand { Id = batchId };

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("trainee");
            exception.Message.ShouldContain("2");
            _batchRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchWithPhases_ThrowsInvalidOperationException()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>
                {
                    new Phase { Id = 1, BatchId = batchId },
                    new Phase { Id = 2, BatchId = batchId }
                },
                TrainingSchedules = new List<TrainingSchedule>()
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());

            var command = new DeleteBatchCommand { Id = batchId };

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("phase");
            exception.Message.ShouldContain("2");
            _batchRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchWithTrainingSchedules_ThrowsInvalidOperationException()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase>(),
                TrainingSchedules = new List<TrainingSchedule>
                {
                    new TrainingSchedule { Id = 1, BatchId = batchId },
                    new TrainingSchedule { Id = 2, BatchId = batchId },
                    new TrainingSchedule { Id = 3, BatchId = batchId }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());

            var command = new DeleteBatchCommand { Id = batchId };

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("training schedule");
            exception.Message.ShouldContain("3");
            _batchRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchWithMultipleRelations_ThrowsExceptionForFirstViolation()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Phases = new List<Phase> { new Phase { Id = 1 } },
                TrainingSchedules = new List<TrainingSchedule> { new TrainingSchedule { Id = 1 } }
            };

            var trainees = new List<Trainee> { new Trainee { Id = 1 } };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(trainees);

            var command = new DeleteBatchCommand { Id = batchId };

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            // Should throw for trainees first (as it's checked first)
            exception.Message.ShouldContain("trainee");
            _batchRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
