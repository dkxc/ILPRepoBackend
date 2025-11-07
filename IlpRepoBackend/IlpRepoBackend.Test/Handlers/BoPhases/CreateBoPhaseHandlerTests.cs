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
    public class CreateBoPhaseHandlerTests
    {
        private readonly Mock<IBoPhaseRepository> _boPhaseRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IBuddyRepository> _buddyRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IDuRepository> _duRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateBoPhaseHandler _handler;

        public CreateBoPhaseHandlerTests()
        {
            _boPhaseRepositoryMock = new Mock<IBoPhaseRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _buddyRepositoryMock = new Mock<IBuddyRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _duRepositoryMock = new Mock<IDuRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateBoPhaseHandler(
                _boPhaseRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _buddyRepositoryMock.Object,
                _userRepositoryMock.Object,
                _duRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesBoPhase()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john.doe@example.com",
                BuddyName = "Jane Smith",
                DuName = "DU1"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john.doe@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1 };
            var boPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BoPhase>())).ReturnsAsync(boPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(boPhase.Id)).ReturnsAsync(boPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.TraineeName.ShouldBe("John Doe");
            result.Data.Buddy.ShouldBe("Jane Smith");
            _boPhaseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BoPhase>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTraineeName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "",
                Email = "john@example.com",
                BuddyName = "Jane"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Trainee name cannot be empty");
        }

        [Fact]
        public async Task Handle_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John",
                Email = "",
                BuddyName = "Jane"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Trainee email cannot be empty");
        }

        [Fact]
        public async Task Handle_EmptyBuddyName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                BuddyName = ""
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Buddy name cannot be empty");
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                BuddyName = "Jane"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_TraineeNameDoesNotMatchEmail_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "jane@example.com",
                BuddyName = "Jane"
            };

            var user = new User { Id = 1, Username = "Jane Smith", Email = "jane@example.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("does not match");
        }

        [Fact]
        public async Task Handle_TraineeProfileNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                BuddyName = "Jane"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Trainee)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Trainee profile");
        }

        [Fact]
        public async Task Handle_TraineeAlreadyHasBoPhase_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                BuddyName = "Jane"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1 };
            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1 };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id))
                .ReturnsAsync(new List<BoPhase> { existingBoPhase });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already has a BO Phase assignment");
        }

        [Fact]
        public async Task Handle_CreatesDuIfNotExists()
        {
            // Arrange
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                BuddyName = "Jane Smith",
                DuName = "NewDU"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var newDu = new Du { Id = 1, Name = "NewDU" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1 };
            var boPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync((Du)null);
            _duRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Du>())).ReturnsAsync(newDu);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BoPhase>())).ReturnsAsync(boPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(boPhase.Id)).ReturnsAsync(boPhase);

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
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                BuddyName = "NewBuddy",
                DuName = "DU1"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var newBuddy = new Buddy { Id = 1, Name = "NewBuddy", DuId = 1 };
            var boPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = newBuddy };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync((Buddy)null);
            _buddyRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Buddy>())).ReturnsAsync(newBuddy);
            _boPhaseRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BoPhase>())).ReturnsAsync(boPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(boPhase.Id)).ReturnsAsync(boPhase);

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
            var command = new CreateBoPhaseCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                BuddyName = "Jane Smith",
                DuName = "DU2"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 2, Name = "DU2" };
            var buddy = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1 }; // Different DU
            var boPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync(command.BuddyName)).ReturnsAsync(buddy);
            _buddyRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Buddy>())).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BoPhase>())).ReturnsAsync(boPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdAsync(boPhase.Id)).ReturnsAsync(boPhase);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _buddyRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Buddy>()), Times.Once);
        }
    }
}
