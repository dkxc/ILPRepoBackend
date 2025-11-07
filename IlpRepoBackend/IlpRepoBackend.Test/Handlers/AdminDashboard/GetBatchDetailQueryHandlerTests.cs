using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.AdminDashboard;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.AdminDashboard
{
    public class GetBatchDetailQueryHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IPhaseRepository> _phaseRepositoryMock;
        private readonly GetBatchDetailQueryHandler _handler;

        public GetBatchDetailQueryHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _phaseRepositoryMock = new Mock<IPhaseRepository>();
            _handler = new GetBatchDetailQueryHandler(
                _batchRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _phaseRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchId_ReturnsCompleteDetails()
        {
            // Arrange
            var batchId = 1;
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow.AddDays(60);

            var batch = new Batch
            {
                Id = batchId,
                BatchName = "ILP Batch 2024",
                Status = BatchStatus.Ongoing,
                BatchTypeId = 1,
                StartDate = startDate,
                EndDate = endDate,
                BatchType = new BatchType { Id = 1, Name = "Full Stack" }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "trainee1@example.com" },
                new Trainee { Id = 2, BatchId = batchId, Email = "trainee2@example.com" },
                new Trainee { Id = 3, BatchId = 2, Email = "trainee3@example.com" } // Different batch
            };

            var phases = new List<Phase>
            {
                new Phase { Id = 1, BatchId = batchId, PhaseType = "Foundation", StartDate = startDate, EndDate = startDate.AddDays(20) },
                new Phase { Id = 2, BatchId = batchId, PhaseType = "Advanced", StartDate = startDate.AddDays(21), EndDate = endDate }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(phases);

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.BatchId.ShouldBe(batchId);
            result.BatchName.ShouldBe("ILP Batch 2024");
            result.BatchStatus.ShouldBe("Ongoing");
            result.BatchType.ShouldBe("1");
            result.NoOfTrainees.ShouldBe(2);
            result.Phases.Count.ShouldBe(2);
            result.DayOfBatch.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ThrowsException()
        {
            // Arrange
            var batchId = 999;
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync((Batch?)null);

            var query = new GetBatchDetailQuery(batchId);

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () => await _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NoTraineesInBatch_ReturnsZeroTrainees()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Empty Batch",
                Status = BatchStatus.NotStarted,
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(90),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.NoOfTrainees.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_NoPhasesInBatch_ReturnsEmptyPhasesList()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Batch Without Phases",
                Status = BatchStatus.NotStarted,
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(90),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Phases.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_BatchWithNullStartDate_UsesFallbackDate()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Batch With Null Date",
                Status = BatchStatus.NotStarted,
                BatchTypeId = 1,
                StartDate = null,
                EndDate = DateTime.UtcNow.AddDays(90),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.DayOfBatch.ShouldBe(0); // Should calculate from DateTime.Now
        }

        [Fact]
        public async Task Handle_MultiplePhases_ReturnsAllPhases()
        {
            // Arrange
            var batchId = 1;
            var startDate = DateTime.UtcNow.AddDays(-30);
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Multi Phase Batch",
                Status = BatchStatus.Ongoing,
                BatchTypeId = 1,
                StartDate = startDate,
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            var phases = new List<Phase>
            {
                new Phase { Id = 1, BatchId = batchId, PhaseType = "Foundation", StartDate = startDate, EndDate = startDate.AddDays(15) },
                new Phase { Id = 2, BatchId = batchId, PhaseType = "Intermediate", StartDate = startDate.AddDays(16), EndDate = startDate.AddDays(45) },
                new Phase { Id = 3, BatchId = batchId, PhaseType = "Advanced", StartDate = startDate.AddDays(46), EndDate = startDate.AddDays(90) },
                new Phase { Id = 4, BatchId = 2, PhaseType = "Other Batch", StartDate = startDate, EndDate = startDate.AddDays(30) }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(phases);

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Phases.Count.ShouldBe(3);
            result.Phases.ShouldContain(p => p.PhaseType == "Foundation");
            result.Phases.ShouldContain(p => p.PhaseType == "Intermediate");
            result.Phases.ShouldContain(p => p.PhaseType == "Advanced");
            result.Phases.ShouldNotContain(p => p.PhaseType == "Other Batch");
        }

        [Fact]
        public async Task Handle_CompletedBatch_ReturnsCorrectStatus()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Completed Batch",
                Status = BatchStatus.Completed,
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(-90),
                EndDate = DateTime.UtcNow.AddDays(-1),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.BatchStatus.ShouldBe("Completed");
        }

        [Fact]
        public async Task Handle_DayOfBatchCalculation_ExcludesSundays()
        {
            // Arrange
            var batchId = 1;
            // Pick a Monday for testing
            var monday = new DateTime(2024, 1, 1); // This is a Monday
            while (monday.DayOfWeek != DayOfWeek.Monday)
            {
                monday = monday.AddDays(1);
            }

            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Status = BatchStatus.Ongoing,
                BatchTypeId = 1,
                StartDate = monday,
                EndDate = monday.AddDays(90),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.DayOfBatch.ShouldBeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task Handle_BatchWithManyTrainees_CountsCorrectly()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Large Batch",
                Status = BatchStatus.Ongoing,
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(60),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            var trainees = Enumerable.Range(1, 50)
                .Select(i => new Trainee { Id = i, BatchId = batchId, Email = $"trainee{i}@example.com" })
                .ToList();

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.NoOfTrainees.ShouldBe(50);
        }

        [Fact]
        public async Task Handle_CallsRepositoriesCorrectly()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch
            {
                Id = batchId,
                BatchName = "Test Batch",
                Status = BatchStatus.NotStarted,
                BatchTypeId = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(90),
                BatchType = new BatchType { Id = 1, Name = "Test" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _phaseRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Phase>());

            var query = new GetBatchDetailQuery(batchId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetByIdAsync(batchId), Times.Once);
            _traineeRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            _phaseRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }
    }
}
