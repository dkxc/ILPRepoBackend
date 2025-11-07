using AutoMapper;
using IlpRepoBackend.Application.Command.BoPhases;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.BoPhases;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.BoPhases
{
    public class UpdateBoPhaseHandlerTests
    {
        private readonly Mock<IBoPhaseRepository> _boPhaseRepositoryMock;
        private readonly Mock<IBuddyRepository> _buddyRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IDuRepository> _duRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateBoPhaseHandler _handler;

        public UpdateBoPhaseHandlerTests()
        {
            _boPhaseRepositoryMock = new Mock<IBoPhaseRepository>();
            _buddyRepositoryMock = new Mock<IBuddyRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _duRepositoryMock = new Mock<IDuRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateBoPhaseHandler(
                _boPhaseRepositoryMock.Object,
                _buddyRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _userRepositoryMock.Object,
                _duRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesBoPhase()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                TraineeName = "John Doe",
                BuddyName = "Jane Smith",
                DuName = "DU1"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var user = new User { Id = 1, Username = "John Doe" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1, Du = du };
            var updatedBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<BoPhase>())).ReturnsAsync(updatedBoPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(updatedBoPhase.Id)).ReturnsAsync(updatedBoPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            _boPhaseRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<BoPhase>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BoPhaseNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 999,
                BuddyName = "Jane Smith"
            };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync((BoPhase)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_TraineeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                TraineeName = "NonExistent",
                BuddyName = "Jane"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_TraineeProfileNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                TraineeName = "John Doe",
                BuddyName = "Jane"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var user = new User { Id = 1, Username = "John Doe" };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Trainee)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Trainee profile");
        }

        [Fact]
        public async Task Handle_TraineeAlreadyHasAnotherBoPhase_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                TraineeName = "John Doe",
                BuddyName = "Jane"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var user = new User { Id = 2, Username = "John Doe" };
            var trainee = new Trainee { Id = 2, UserId = 2 };
            var anotherBoPhase = new BoPhase { Id = 2, TraineeId = 2 };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id))
                .ReturnsAsync(new List<BoPhase> { anotherBoPhase });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already has a BO Phase assignment");
        }

        [Fact]
        public async Task Handle_AllowsSameTraineeUpdate()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                TraineeName = "John Doe",
                BuddyName = "Jane Smith",
                DuName = "DU1"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var user = new User { Id = 1, Username = "John Doe" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1, Du = du };
            var updatedBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<BoPhase>())).ReturnsAsync(updatedBoPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(updatedBoPhase.Id)).ReturnsAsync(updatedBoPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_CreatesDuIfNotExists()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                BuddyName = "Jane Smith",
                DuName = "NewDU"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var newDu = new Du { Id = 2, Name = "NewDU" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1 };
            var trainee = new Trainee { Id = 1, User = new User { Username = "John" } };
            var updatedBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync((Du)null);
            _duRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Du>())).ReturnsAsync(newDu);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _buddyRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Buddy>())).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<BoPhase>())).ReturnsAsync(updatedBoPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(updatedBoPhase.Id)).ReturnsAsync(updatedBoPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _duRepositoryMock.Verify(x => x.AddAsync(It.Is<Du>(d => d.Name == "NewDU")), Times.Once);
        }

        [Fact]
        public async Task Handle_CreatesBuddyIfNotExists()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                BuddyName = "NewBuddy",
                DuName = "DU1"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var du = new Du { Id = 1, Name = "DU1" };
            var newBuddy = new Buddy { Id = 2, Name = "NewBuddy", DuId = 1, Du = du };
            var trainee = new Trainee { Id = 1, User = new User { Username = "John" } };
            var updatedBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 2, Trainee = trainee, Buddy = newBuddy };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync((Buddy)null);
            _buddyRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Buddy>())).ReturnsAsync(newBuddy);
            _boPhaseRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<BoPhase>())).ReturnsAsync(updatedBoPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(updatedBoPhase.Id)).ReturnsAsync(updatedBoPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _buddyRepositoryMock.Verify(x => x.AddAsync(It.Is<Buddy>(b => b.Name == "NewBuddy")), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdatesBuddyDuIfChanged()
        {
            // Arrange
            var command = new UpdateBoPhaseCommand
            {
                BoPhaseId = 1,
                BuddyName = "Jane Smith",
                DuName = "DU2"
            };

            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1 };
            var du = new Du { Id = 2, Name = "DU2" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1, Du = du };
            var trainee = new Trainee { Id = 1, User = new User { Username = "John" } };
            var updatedBoPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(command.BoPhaseId)).ReturnsAsync(existingBoPhase);
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _buddyRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Buddy>())).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<BoPhase>())).ReturnsAsync(updatedBoPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(updatedBoPhase.Id)).ReturnsAsync(updatedBoPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _buddyRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Buddy>()), Times.Once);
        }
    }
}
