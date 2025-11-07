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
    public class CreateTraineeHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly CreateTraineeHandler _handler;

        public CreateTraineeHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _handler = new CreateTraineeHandler(
                _mapperMock.Object,
                _userRepositoryMock.Object,
                _traineeRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesTrainee()
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Username = "john.doe",
                Email = "john@example.com",
                Password = "Password123!",
                BatchId = 1,
                PhoneNo = "1234567890",
                Status = TraineeStatus.Active
            };

            var user = new User { Id = 1, Username = command.Username, Email = command.Email };
            var trainee = new Trainee { Id = 1, UserId = 1, Email = command.Email };
            var traineeDto = new TraineeDto { Id = 1, Email = command.Email };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email)).ReturnsAsync(false);
            _traineeRepositoryMock.Setup(x => x.AadhaarIdExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>())).ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(trainee)).Returns(traineeDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _traineeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Trainee>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Email = "existing@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Email already exists");
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AadhaarIdAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Email = "test@example.com",
                Password = "Password123!",
                AadhaarId = "123456789012"
            };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email)).ReturnsAsync(false);
            _traineeRepositoryMock.Setup(x => x.AadhaarIdExistsAsync(command.AadhaarId)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Aadhaar ID");
            result.Message.ShouldContain("already exists");
        }

        [Fact]
        public async Task Handle_HashesPassword_BeforeCreatingUser()
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Username = "test",
                Email = "test@example.com",
                Password = "PlainPassword123",
                BatchId = 1
            };

            User? capturedUser = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync(new User { Id = 1 });
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>()))
                .ReturnsAsync(new Trainee { Id = 1 });
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.PasswordHash.ShouldNotBe(command.Password);
            capturedUser.PasswordHash.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_SetsUserRoleToTrainee()
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Username = "test",
                Email = "test@example.com",
                Password = "Password123!",
                BatchId = 1
            };

            User? capturedUser = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync(new User { Id = 1 });
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>()))
                .ReturnsAsync(new Trainee { Id = 1 });
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.Role.ShouldBe(UserRole.Trainee);
            capturedUser.IsActive.ShouldBeTrue();
        }

        [Theory]
        [InlineData(TraineeStatus.Active)]
        [InlineData(TraineeStatus.Inactive)]
        public async Task Handle_DifferentStatuses_CreatesCorrectly(TraineeStatus status)
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Username = "test",
                Email = "test@example.com",
                Password = "Password123!",
                BatchId = 1,
                Status = status
            };

            Trainee? capturedTrainee = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(new User { Id = 1 });
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>()))
                .Callback<Trainee>(t => capturedTrainee = t)
                .ReturnsAsync(new Trainee { Id = 1 });
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>())).Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedTrainee.ShouldNotBeNull();
            capturedTrainee.Status.ShouldBe(status);
        }

        [Fact]
        public async Task Handle_IncludesUsernameInResponse()
        {
            // Arrange
            var command = new CreateTraineeCommand
            {
                Username = "john.doe",
                Email = "john@example.com",
                Password = "Password123!",
                BatchId = 1
            };

            var user = new User { Id = 1, Username = command.Username };
            var trainee = new Trainee { Id = 1, UserId = 1 };
            var traineeDto = new TraineeDto { Id = 1 };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>())).ReturnsAsync(trainee);
            _mapperMock.Setup(x => x.Map<TraineeDto>(trainee)).Returns(traineeDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Data.Username.ShouldBe(command.Username);
        }
    }
}
