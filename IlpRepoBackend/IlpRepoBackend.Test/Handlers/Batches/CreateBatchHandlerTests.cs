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
    public class CreateBatchHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<ITrainingScheduleRepository> _trainingScheduleRepositoryMock;
        private readonly CreateBatchHandler _handler;

        public CreateBatchHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _trainingScheduleRepositoryMock = new Mock<ITrainingScheduleRepository>();
            _handler = new CreateBatchHandler(
                _mapperMock.Object,
                _batchRepositoryMock.Object,
                _trainingScheduleRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesBatch()
        {
            // Arrange
            var command = new CreateBatchCommand
            {
                BatchName = "Test Batch",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(15),
                Phases = new List<PhaseDto>()
            };

            var createdBatch = new Batch
            {
                Id = 1,
                BatchName = command.BatchName,
                BatchTypeId = command.BatchTypeId,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                Status = BatchStatus.NotStarted
            };

            var batchDto = new BatchDto
            {
                Id = 1,
                BatchName = command.BatchName,
                Status = BatchStatus.NotStarted
            };

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .ReturnsAsync(createdBatch);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(createdBatch.Id))
                .ReturnsAsync(createdBatch);
            _trainingScheduleRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TrainingSchedule>>()))
                .Returns(Task.CompletedTask);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(batchDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(1);
            result.BatchName.ShouldBe(command.BatchName);
            _batchRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Batch>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BatchNotStarted_StatusIsNotStarted()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(10);
            var command = new CreateBatchCommand
            {
                BatchName = "Future Batch",
                BatchTypeId = 1,
                StartDate = futureDate,
                EndDate = futureDate.AddDays(30)
            };

            var createdBatch = new Batch
            {
                Id = 1,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                Status = BatchStatus.NotStarted
            };

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .ReturnsAsync(createdBatch);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(createdBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto { Status = BatchStatus.NotStarted });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.ShouldBe(BatchStatus.NotStarted);
        }

        [Fact]
        public async Task Handle_BatchOngoing_StatusIsOngoing()
        {
            // Arrange
            var command = new CreateBatchCommand
            {
                BatchName = "Current Batch",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var createdBatch = new Batch
            {
                Id = 1,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                Status = BatchStatus.Ongoing
            };

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .ReturnsAsync(createdBatch);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(createdBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto { Status = BatchStatus.Ongoing });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.ShouldBe(BatchStatus.Ongoing);
        }

        [Fact]
        public async Task Handle_BatchCompleted_StatusIsCompleted()
        {
            // Arrange
            var command = new CreateBatchCommand
            {
                BatchName = "Past Batch",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-5)
            };

            var createdBatch = new Batch
            {
                Id = 1,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                Status = BatchStatus.Completed
            };

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .ReturnsAsync(createdBatch);
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(createdBatch);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto { Status = BatchStatus.Completed });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.ShouldBe(BatchStatus.Completed);
        }

        [Fact]
        public async Task Handle_WithPhases_CreatesPhases()
        {
            // Arrange
            var command = new CreateBatchCommand
            {
                BatchName = "Batch with Phases",
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Phases = new List<PhaseDto>
                {
                    new PhaseDto { PhaseType = "Foundation", PhaseTypeId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10) },
                    new PhaseDto { PhaseType = "Advanced", PhaseTypeId = 2, StartDate = DateTime.UtcNow.AddDays(11), EndDate = DateTime.UtcNow.AddDays(20) }
                }
            };

            Batch? capturedBatch = null;
            var createdBatch = new Batch { Id = 1, BatchName = command.BatchName, StartDate = command.StartDate, EndDate = command.EndDate };

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .Callback<Batch>(b => capturedBatch = b)
                .ReturnsAsync((Batch b) => { b.Id = 1; return b; });
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(createdBatch);
            _trainingScheduleRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TrainingSchedule>>()))
                .Returns(Task.CompletedTask);
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
        public async Task Handle_WithValidDates_GeneratesTrainingSchedules()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 5);
            var command = new CreateBatchCommand
            {
                BatchName = "Test Batch",
                BatchTypeId = 1,
                StartDate = startDate,
                EndDate = endDate
            };

            IEnumerable<TrainingSchedule>? capturedSchedules = null;

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .ReturnsAsync((Batch b) => { b.Id = 1; b.StartDate = startDate; b.EndDate = endDate; return b; });
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Batch { Id = 1, StartDate = startDate, EndDate = endDate });
            _trainingScheduleRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TrainingSchedule>>()))
                .Callback<IEnumerable<TrainingSchedule>>(s => capturedSchedules = s)
                .Returns(Task.CompletedTask);
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedSchedules.ShouldNotBeNull();
            capturedSchedules.Count().ShouldBe(5); // 5 days from Jan 1 to Jan 5
            capturedSchedules.All(s => s.Hours == 8).ShouldBeTrue();
            capturedSchedules.All(s => s.BatchId == 1).ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_WithoutDates_DoesNotGenerateTrainingSchedules()
        {
            // Arrange
            var command = new CreateBatchCommand
            {
                BatchName = "Batch without dates",
                BatchTypeId = 1,
                StartDate = null,
                EndDate = null
            };

            _batchRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Batch>()))
                .ReturnsAsync((Batch b) => { b.Id = 1; return b; });
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Batch { Id = 1, StartDate = null, EndDate = null });
            _mapperMock.Setup(x => x.Map<BatchDto>(It.IsAny<Batch>()))
                .Returns(new BatchDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _trainingScheduleRepositoryMock.Verify(
                x => x.AddRangeAsync(It.IsAny<List<TrainingSchedule>>()),
                Times.Never);
        }
    }
}
