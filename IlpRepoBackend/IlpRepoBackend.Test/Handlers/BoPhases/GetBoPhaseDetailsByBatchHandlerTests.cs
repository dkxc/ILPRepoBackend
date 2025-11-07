using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.BoPhases;
using IlpRepoBackend.Application.Query.BoPhases;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.BoPhases
{
    public class GetBoPhaseDetailsByBatchHandlerTests
    {
        private readonly Mock<IBoPhaseRepository> _boPhaseRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetBoPhaseDetailsByBatchHandler _handler;

        public GetBoPhaseDetailsByBatchHandlerTests()
        {
            _boPhaseRepositoryMock = new Mock<IBoPhaseRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetBoPhaseDetailsByBatchHandler(
                _boPhaseRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchId_ReturnsBoPhaseDetails()
        {
            // Arrange
            var batchId = 1;
            var query = new GetBoPhaseDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId },
                new Trainee { Id = 2, BatchId = batchId }
            };

            var boPhases = new List<BoPhase>
            {
                new BoPhase
                {
                    Id = 1,
                    TraineeId = 1,
                    BuddyId = 1,
                    Trainee = new Trainee
                    {
                        Id = 1,
                        User = new User { Username = "John Doe" }
                    },
                    Buddy = new Buddy
                    {
                        Id = 1,
                        Name = "Jane Smith",
                        Du = new Du { Name = "DU1" }
                    }
                },
                new BoPhase
                {
                    Id = 2,
                    TraineeId = 2,
                    BuddyId = 2,
                    Trainee = new Trainee
                    {
                        Id = 2,
                        User = new User { Username = "Alice Johnson" }
                    },
                    Buddy = new Buddy
                    {
                        Id = 2,
                        Name = "Bob Wilson",
                        Du = new Du { Name = "DU2" }
                    }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _boPhaseRepositoryMock.Setup(x => x.GetBoPhaseDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(boPhases);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].TraineeName.ShouldBe("John Doe");
            result.Data[0].Buddy.ShouldBe("Jane Smith");
            result.Data[0].BuddyDU.ShouldBe("DU1");
            result.Data[1].TraineeName.ShouldBe("Alice Johnson");
        }

        [Fact]
        public async Task Handle_NoTraineesInBatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var query = new GetBoPhaseDetailsByBatchQuery(batchId);

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No trainees found");
        }

        [Fact]
        public async Task Handle_NoBoPhaseAssignments_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var query = new GetBoPhaseDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId },
                new Trainee { Id = 2, BatchId = batchId }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _boPhaseRepositoryMock.Setup(x => x.GetBoPhaseDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<BoPhase>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No BO Phase assignments found");
        }

        [Fact]
        public async Task Handle_PassesCorrectTraineeIds()
        {
            // Arrange
            var batchId = 1;
            var query = new GetBoPhaseDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 5, BatchId = batchId },
                new Trainee { Id = 10, BatchId = batchId },
                new Trainee { Id = 15, BatchId = batchId }
            };

            var boPhases = new List<BoPhase>
            {
                new BoPhase
                {
                    Id = 1,
                    TraineeId = 5,
                    Trainee = new Trainee { Id = 5, User = new User { Username = "User1" } },
                    Buddy = new Buddy { Name = "Buddy1" }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _boPhaseRepositoryMock.Setup(x => x.GetBoPhaseDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(boPhases);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _boPhaseRepositoryMock.Verify(x => x.GetBoPhaseDetailsByTraineeIds(
                It.Is<List<int>>(ids => ids.Contains(5) && ids.Contains(10) && ids.Contains(15))), Times.Once);
        }

        [Fact]
        public async Task Handle_MapsNullBuddyDuCorrectly()
        {
            // Arrange
            var batchId = 1;
            var query = new GetBoPhaseDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId }
            };

            var boPhases = new List<BoPhase>
            {
                new BoPhase
                {
                    Id = 1,
                    TraineeId = 1,
                    Trainee = new Trainee { Id = 1, User = new User { Username = "John" } },
                    Buddy = new Buddy { Name = "Jane", Du = null }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _boPhaseRepositoryMock.Setup(x => x.GetBoPhaseDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(boPhases);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].BuddyDU.ShouldBeNull();
        }
    }
}
