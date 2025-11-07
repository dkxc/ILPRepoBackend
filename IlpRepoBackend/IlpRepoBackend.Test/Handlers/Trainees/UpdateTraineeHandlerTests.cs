using AutoMapper;
using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Trainees;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Trainees
{
    public class UpdateTraineeHandlerTests
    {
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateTraineeHandler _handler;

        public UpdateTraineeHandlerTests()
        {
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateTraineeHandler(
                _traineeRepositoryMock.Object,
                _userRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesTrainee()
        {
            // Arrange
            var command = new UpdateTraineeCommand
            {
                Id = 1,
                Username = "updated.user",
                Email = "updated@example.com",
                PhoneNo = "9876543210",
                Status = TraineeStatus.Active
            };

            var trainee = new Trainee { Id = 1, UserId = 1, Email = "old@example.com" };
            var user = new User { Id = 1, Username = "old.user", Email = "old@example.com" };
            var traineeDto = new TraineeDto { Id = 1 };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Trainee>())).ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(trainee)).Returns(traineeDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            _traineeRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Trainee>()), Times.Once);
        }

        [Fact]
        public async Task Handle_TraineeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateTraineeCommand { Id = 999 };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Trainee?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateTraineeCommand { Id = 1 };
            var trainee = new Trainee { Id = 1, UserId = 1 };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("User account");
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateTraineeCommand
            {
                Id = 1,
                Email = "existing@example.com"
            };

            var trainee = new Trainee { Id = 1, UserId = 1, Email = "old@example.com" };
            var user = new User { Id = 1, Email = "old@example.com" };

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Email already exists");
        }

        [Fact]
        public async Task Handle_UpdatesUsernameInUser()
        {
            // Arrange
            var command = new UpdateTraineeCommand
            {
                Id = 1,
                Username = "new.username"
            };

            var trainee = new Trainee { Id = 1, UserId = 1 };
            var user = new User { Id = 1, Username = "old.username" };
            User? capturedUser = null;

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Trainee>())).ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.Username.ShouldBe(command.Username);
        }

        [Fact]
        public async Task Handle_UpdatesEmailInBothUserAndTrainee()
        {
            // Arrange
            var command = new UpdateTraineeCommand
            {
                Id = 1,
                Email = "new@example.com"
            };

            var trainee = new Trainee { Id = 1, UserId = 1, Email = "old@example.com" };
            var user = new User { Id = 1, Email = "old@example.com" };
            User? capturedUser = null;
            Trainee? capturedTrainee = null;

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Trainee>()))
                .Callback<Trainee>(t => capturedTrainee = t)
                .ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.Email.ShouldBe(command.Email);
            capturedTrainee.ShouldNotBeNull();
            capturedTrainee.Email.ShouldBe(command.Email);
        }

        [Fact]
        public async Task Handle_UpdatesTimestamps()
        {
            // Arrange
            var command = new UpdateTraineeCommand
            {
                Id = 1,
                Email = "test@example.com"
            };

            var trainee = new Trainee { Id = 1, UserId = 1, UpdatedAt = DateTime.UtcNow.AddDays(-1) };
            var user = new User { Id = 1, UpdatedAt = DateTime.UtcNow.AddDays(-1) };
            var oldTraineeTimestamp = trainee.UpdatedAt;
            var oldUserTimestamp = user.UpdatedAt;

            User? capturedUser = null;
            Trainee? capturedTrainee = null;

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Trainee>()))
                .Callback<Trainee>(t => capturedTrainee = t)
                .ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.UpdatedAt.ShouldBeGreaterThan(oldUserTimestamp);
            capturedTrainee.ShouldNotBeNull();
            capturedTrainee.UpdatedAt.ShouldBeGreaterThan(oldTraineeTimestamp);
        }

        [Theory]
        [InlineData(TraineeStatus.Active)]
        [InlineData(TraineeStatus.Inactive)]
        public async Task Handle_UpdatesStatus(TraineeStatus status)
        {
            // Arrange
            var command = new UpdateTraineeCommand
            {
                Id = 1,
                Status = status
            };

            var trainee = new Trainee { Id = 1, UserId = 1, Status = TraineeStatus.Active };
            var user = new User { Id = 1 };
            Trainee? capturedTrainee = null;

            _traineeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Trainee>()))
                .Callback<Trainee>(t => capturedTrainee = t)
                .ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedTrainee.ShouldNotBeNull();
            capturedTrainee.Status.ShouldBe(status);
        }
    }
}
