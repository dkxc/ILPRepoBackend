using AutoMapper;
using IlpRepoBackend.Application.Command.TraineeDus;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.TraineeDus;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.TraineeDus
{
    public class CreateTraineeDuHandlerTests
    {
        private readonly Mock<ITraineeDuRepository> _traineeDuRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IDuRepository> _duRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateTraineeDuHandler _handler;

        public CreateTraineeDuHandlerTests()
        {
            _traineeDuRepositoryMock = new Mock<ITraineeDuRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _duRepositoryMock = new Mock<IDuRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateTraineeDuHandler(
                _traineeDuRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _duRepositoryMock.Object,
                _userRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesTraineeDu()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John Doe",
                Email = "john@example.com",
                DuName = "DU1",
                Location = "Bangalore",
                OjtMenter = "Mr. Smith"
            };

            var user = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var traineeDu = new TraineeDu
            {
                Id = 1,
                TraineeId = 1,
                DuId = 1,
                Location = "Bangalore",
                ojtMenter = "Mr. Smith",
                Trainee = trainee,
                Du = du
            };
            var traineeDuDto = new TraineeDuDto
            {
                Id = 1,
                TraineeId = 1,
                DuId = 1,
                TraineeName = "John Doe",
                DuName = "DU1",
                Location = "Bangalore",
                OjtMenter = "Mr. Smith"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<TraineeDu>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _traineeDuRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TraineeDu>())).ReturnsAsync(traineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(traineeDu.Id)).ReturnsAsync(traineeDu);
            _mapperMock.Setup(x => x.Map<TraineeDuDto>(traineeDu)).Returns(traineeDuDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.TraineeName.ShouldBe("John Doe");
            result.Data.DuName.ShouldBe("DU1");
            result.Data.Location.ShouldBe("Bangalore");
            result.Data.OjtMenter.ShouldBe("Mr. Smith");
            _traineeDuRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TraineeDu>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTraineeName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "",
                Email = "john@example.com",
                DuName = "DU1"
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
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "",
                DuName = "DU1"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Trainee email cannot be empty");
        }

        [Fact]
        public async Task Handle_EmptyDuName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                DuName = ""
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("DU name cannot be empty");
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                DuName = "DU1"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_TraineeNameMismatch_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "jane@example.com",
                DuName = "DU1"
            };

            var user = new User { Id = 1, Username = "Jane", Email = "jane@example.com" };
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
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                DuName = "DU1"
            };

            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Trainee)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Trainee profile");
        }

        [Fact]
        public async Task Handle_TraineeAlreadyHasDuAssignment_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                DuName = "DU1"
            };

            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1 };
            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1 };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id))
                .ReturnsAsync(new List<TraineeDu> { existingTraineeDu });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already has a DU assignment");
        }

        [Fact]
        public async Task Handle_CreatesDuIfNotExists()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                DuName = "NewDU",
                Location = "Mumbai",
                OjtMenter = "Mr. Kumar"
            };

            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var newDu = new Du { Id = 1, Name = "NewDU" };
            var traineeDu = new TraineeDu { Id = 1, TraineeId = 1, DuId = 1, Trainee = trainee, Du = newDu };
            var traineeDuDto = new TraineeDuDto { Id = 1 };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<TraineeDu>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync((Du)null);
            _duRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Du>())).ReturnsAsync(newDu);
            _traineeDuRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TraineeDu>())).ReturnsAsync(traineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(traineeDu.Id)).ReturnsAsync(traineeDu);
            _mapperMock.Setup(x => x.Map<TraineeDuDto>(traineeDu)).Returns(traineeDuDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _duRepositoryMock.Verify(x => x.AddAsync(It.Is<Du>(d => d.Name == "NewDU")), Times.Once);
        }

        [Fact]
        public async Task Handle_SetsLocationAndOjtMenter()
        {
            // Arrange
            var command = new CreateTraineeDuCommand
            {
                TraineeName = "John",
                Email = "john@example.com",
                DuName = "DU1",
                Location = "Hyderabad",
                OjtMenter = "Ms. Sharma"
            };

            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var traineeDu = new TraineeDu
            {
                Id = 1,
                TraineeId = 1,
                DuId = 1,
                Location = "Hyderabad",
                ojtMenter = "Ms. Sharma",
                Trainee = trainee,
                Du = du
            };
            var traineeDuDto = new TraineeDuDto
            {
                Id = 1,
                Location = "Hyderabad",
                OjtMenter = "Ms. Sharma"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<TraineeDu>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuName)).ReturnsAsync(du);
            _traineeDuRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TraineeDu>())).ReturnsAsync(traineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(traineeDu.Id)).ReturnsAsync(traineeDu);
            _mapperMock.Setup(x => x.Map<TraineeDuDto>(traineeDu)).Returns(traineeDuDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _traineeDuRepositoryMock.Verify(x => x.AddAsync(It.Is<TraineeDu>(
                td => td.Location == "Hyderabad" && td.ojtMenter == "Ms. Sharma")), Times.Once);
        }
    }
}
