using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.TraineeDus;
using IlpRepoBackend.Application.Query.TraineeDus;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.TraineeDus
{
    public class GetTraineeDuDetailsByBatchHandlerTests
    {
        private readonly Mock<ITraineeDuRepository> _traineeDuRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetTraineeDuDetailsByBatchHandler _handler;

        public GetTraineeDuDetailsByBatchHandlerTests()
        {
            _traineeDuRepositoryMock = new Mock<ITraineeDuRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetTraineeDuDetailsByBatchHandler(
                _traineeDuRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchId_ReturnsTraineeDuDetails()
        {
            // Arrange
            var batchId = 1;
            var query = new GetTraineeDuDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId },
                new Trainee { Id = 2, BatchId = batchId }
            };

            var traineeDus = new List<TraineeDu>
            {
                new TraineeDu
                {
                    Id = 1,
                    TraineeId = 1,
                    DuId = 1,
                    Location = "Bangalore",
                    ojtMenter = "Mr. Smith",
                    Trainee = new Trainee { Id = 1, User = new User { Username = "John Doe" } },
                    Du = new Du { Id = 1, Name = "DU1" }
                },
                new TraineeDu
                {
                    Id = 2,
                    TraineeId = 2,
                    DuId = 2,
                    Location = "Mumbai",
                    ojtMenter = "Ms. Kumar",
                    Trainee = new Trainee { Id = 2, User = new User { Username = "Alice Johnson" } },
                    Du = new Du { Id = 2, Name = "DU2" }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _traineeDuRepositoryMock.Setup(x => x.GetTraineeDuDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(traineeDus);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].TraineeName.ShouldBe("John Doe");
            result.Data[0].DuAllocated.ShouldBe("DU1");
            result.Data[0].Location.ShouldBe("Bangalore");
            result.Data[0].OjtMentor.ShouldBe("Mr. Smith");
            result.Data[1].TraineeName.ShouldBe("Alice Johnson");
        }

        [Fact]
        public async Task Handle_NoTraineesInBatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var query = new GetTraineeDuDetailsByBatchQuery(batchId);

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No trainees found");
        }

        [Fact]
        public async Task Handle_NoDuAssignments_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var query = new GetTraineeDuDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId },
                new Trainee { Id = 2, BatchId = batchId }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _traineeDuRepositoryMock.Setup(x => x.GetTraineeDuDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<TraineeDu>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No DU assignments found");
        }

        [Fact]
        public async Task Handle_PassesCorrectTraineeIds()
        {
            // Arrange
            var batchId = 1;
            var query = new GetTraineeDuDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 5, BatchId = batchId },
                new Trainee { Id = 10, BatchId = batchId },
                new Trainee { Id = 15, BatchId = batchId }
            };

            var traineeDus = new List<TraineeDu>
            {
                new TraineeDu
                {
                    Id = 1,
                    TraineeId = 5,
                    Trainee = new Trainee { Id = 5, User = new User { Username = "User1" } },
                    Du = new Du { Name = "DU1" }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _traineeDuRepositoryMock.Setup(x => x.GetTraineeDuDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(traineeDus);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _traineeDuRepositoryMock.Verify(x => x.GetTraineeDuDetailsByTraineeIds(
                It.Is<List<int>>(ids => ids.Contains(5) && ids.Contains(10) && ids.Contains(15))), Times.Once);
        }

        [Fact]
        public async Task Handle_MapsNullLocationCorrectly()
        {
            // Arrange
            var batchId = 1;
            var query = new GetTraineeDuDetailsByBatchQuery(batchId);

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId }
            };

            var traineeDus = new List<TraineeDu>
            {
                new TraineeDu
                {
                    Id = 1,
                    TraineeId = 1,
                    Location = null,
                    Trainee = new Trainee { Id = 1, User = new User { Username = "John" } },
                    Du = new Du { Name = "DU1" }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _traineeDuRepositoryMock.Setup(x => x.GetTraineeDuDetailsByTraineeIds(It.IsAny<List<int>>()))
                .ReturnsAsync(traineeDus);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Location.ShouldBeNull();
        }
    }
}
