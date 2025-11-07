using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Trainees;
using IlpRepoBackend.Application.Query.Trainees;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Trainees
{
    public class GetTraineeTrainingDetailsHandlerTests
    {
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IBoPhaseRepository> _boPhaseRepositoryMock;
        private readonly Mock<ITraineeDuRepository> _traineeDuRepositoryMock;
        private readonly GetTraineeTrainingDetailsHandler _handler;

        public GetTraineeTrainingDetailsHandlerTests()
        {
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _boPhaseRepositoryMock = new Mock<IBoPhaseRepository>();
            _traineeDuRepositoryMock = new Mock<ITraineeDuRepository>();
            _handler = new GetTraineeTrainingDetailsHandler(
                _traineeRepositoryMock.Object,
                _boPhaseRepositoryMock.Object,
                _traineeDuRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidTraineeId_ReturnsTrainingDetails()
        {
            // Arrange
            var traineeId = 1;
            var trainee = new Trainee
            {
                Id = traineeId,
                Email = "trainee@example.com",
                User = new User { Username = "john.doe" },
                Batch = new Batch { BatchName = "Batch 1" }
            };

            var boPhase = new BoPhase
            {
                TraineeId = traineeId,
                Buddy = new Buddy
                {
                    Name = "Buddy Name",
                    Du = new Du { Name = "DU Name" }
                }
            };

            var traineeDu = new TraineeDu
            {
                TraineeId = traineeId,
                ojtMenter = "OJT Mentor",
                Du = new Du { Name = "Training DU" },
                Location = "Bangalore"
            };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<BoPhase> { boPhase });
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<TraineeDu> { traineeDu });

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.TraineeId.ShouldBe(traineeId);
            result.Data.TraineeName.ShouldBe("john.doe");
            result.Data.Email.ShouldBe("trainee@example.com");
            result.Data.BatchName.ShouldBe("Batch 1");
            result.Data.BuddyName.ShouldBe("Buddy Name");
            result.Data.BuddyDU.ShouldBe("DU Name");
            result.Data.OjtMentor.ShouldBe("OJT Mentor");
            result.Data.DuAllocated.ShouldBe("Training DU");
            result.Data.Location.ShouldBe("Bangalore");
        }

        [Fact]
        public async Task Handle_TraineeNotFound_ReturnsFailure()
        {
            // Arrange
            var traineeId = 999;
            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync((Trainee?)null);

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            result.Message.ShouldContain("999");
        }

        [Fact]
        public async Task Handle_NoBoPhaseData_ReturnsTrainingDetailsWithNullBuddyInfo()
        {
            // Arrange
            var traineeId = 1;
            var trainee = new Trainee
            {
                Id = traineeId,
                User = new User { Username = "john.doe" },
                Batch = new Batch { BatchName = "Batch 1" }
            };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<BoPhase>());
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<TraineeDu>());

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.BuddyName.ShouldBeNull();
            result.Data.BuddyDU.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_NoTraineeDuData_ReturnsTrainingDetailsWithNullDuInfo()
        {
            // Arrange
            var traineeId = 1;
            var trainee = new Trainee
            {
                Id = traineeId,
                User = new User { Username = "john.doe" },
                Batch = new Batch { BatchName = "Batch 1" }
            };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<BoPhase>());
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<TraineeDu>());

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.OjtMentor.ShouldBeNull();
            result.Data.DuAllocated.ShouldBeNull();
            result.Data.Location.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_HandlesNullUserGracefully()
        {
            // Arrange
            var traineeId = 1;
            var trainee = new Trainee
            {
                Id = traineeId,
                Email = "test@example.com",
                User = null,
                Batch = new Batch { BatchName = "Batch 1" }
            };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<BoPhase>());
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<TraineeDu>());

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.TraineeName.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Handle_HandlesNullBatchGracefully()
        {
            // Arrange
            var traineeId = 1;
            var trainee = new Trainee
            {
                Id = traineeId,
                Email = "test@example.com",
                User = new User { Username = "john.doe" },
                Batch = null
            };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<BoPhase>());
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<TraineeDu>());

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.BatchName.ShouldBe(string.Empty);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Handle_DifferentTraineeIds_CallsRepositoryWithCorrectId(int traineeId)
        {
            // Arrange
            var trainee = new Trainee
            {
                Id = traineeId,
                User = new User { Username = "test" },
                Batch = new Batch { BatchName = "Batch" }
            };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(traineeId)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<BoPhase>());
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(traineeId))
                .ReturnsAsync(new List<TraineeDu>());

            var query = new GetTraineeTrainingDetailsQuery(traineeId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _traineeRepositoryMock.Verify(x => x.GetByIdAsync(traineeId), Times.Once);
            _boPhaseRepositoryMock.Verify(x => x.GetByTraineeIdAsync(traineeId), Times.Once);
            _traineeDuRepositoryMock.Verify(x => x.GetByTraineeIdAsync(traineeId), Times.Once);
        }
    }
}
