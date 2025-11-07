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
    public class UpdateTraineeDuHandlerTests
    {
        private readonly Mock<ITraineeDuRepository> _traineeDuRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IDuRepository> _duRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateTraineeDuHandler _handler;

        public UpdateTraineeDuHandlerTests()
        {
            _traineeDuRepositoryMock = new Mock<ITraineeDuRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _duRepositoryMock = new Mock<IDuRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateTraineeDuHandler(
                _traineeDuRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _userRepositoryMock.Object,
                _duRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesTraineeDu()
        {
            // Arrange
            var command = new UpdateTraineeDuCommand
            {
                TraineeDuId = 1,
                TraineeName = "John Doe",
                DuAllocated = "DU2",
                Location = "Chennai",
                OjtMentor = "Mr. Reddy"
            };

            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1, DuId = 1 };
            var user = new User { Id = 1, Username = "John Doe" };
            var trainee = new Trainee { Id = 1, UserId = 1, User = user };
            var du = new Du { Id = 2, Name = "DU2" };
            var updatedTraineeDu = new TraineeDu
            {
                Id = 1,
                TraineeId = 1,
                DuId = 2,
                Location = "Chennai",
                ojtMenter = "Mr. Reddy",
                Trainee = trainee,
                Du = du
            };

            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(command.TraineeDuId)).ReturnsAsync(existingTraineeDu);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id)).ReturnsAsync(new List<TraineeDu>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuAllocated)).ReturnsAsync(du);
            _traineeDuRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TraineeDu>())).ReturnsAsync(updatedTraineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(updatedTraineeDu.Id)).ReturnsAsync(updatedTraineeDu);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.DuAllocated.ShouldBe("DU2");
            result.Data.Location.ShouldBe("Chennai");
            result.Data.OjtMentor.ShouldBe("Mr. Reddy");
            _traineeDuRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TraineeDu>()), Times.Once);
        }

        [Fact]
        public async Task Handle_TraineeDuNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateTraineeDuCommand
            {
                TraineeDuId = 999,
                DuAllocated = "DU1"
            };

            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(command.TraineeDuId)).ReturnsAsync((TraineeDu)null);

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
            var command = new UpdateTraineeDuCommand
            {
                TraineeDuId = 1,
                TraineeName = "NonExistent",
                DuAllocated = "DU1"
            };

            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1 };
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(command.TraineeDuId)).ReturnsAsync(existingTraineeDu);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_TraineeHasAnotherDuAssignment_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateTraineeDuCommand
            {
                TraineeDuId = 1,
                TraineeName = "John Doe",
                DuAllocated = "DU1"
            };

            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1 };
            var user = new User { Id = 2, Username = "John Doe" };
            var trainee = new Trainee { Id = 2, UserId = 2 };
            var anotherTraineeDu = new TraineeDu { Id = 2, TraineeId = 2 };

            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(command.TraineeDuId)).ReturnsAsync(existingTraineeDu);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(command.TraineeName)).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(trainee.Id))
                .ReturnsAsync(new List<TraineeDu> { anotherTraineeDu });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already has a different DU assignment");
        }

        [Fact]
        public async Task Handle_CreatesDuIfNotExists()
        {
            // Arrange
            var command = new UpdateTraineeDuCommand
            {
                TraineeDuId = 1,
                DuAllocated = "NewDU",
                Location = "Pune",
                OjtMentor = "Mr. Patel"
            };

            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1, DuId = 1 };
            var trainee = new Trainee { Id = 1, User = new User { Username = "John" } };
            var newDu = new Du { Id = 2, Name = "NewDU" };
            var updatedTraineeDu = new TraineeDu
            {
                Id = 1,
                TraineeId = 1,
                DuId = 2,
                Trainee = trainee,
                Du = newDu
            };

            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(command.TraineeDuId)).ReturnsAsync(existingTraineeDu);
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuAllocated)).ReturnsAsync((Du)null);
            _duRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Du>())).ReturnsAsync(newDu);
            _traineeDuRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TraineeDu>())).ReturnsAsync(updatedTraineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(updatedTraineeDu.Id)).ReturnsAsync(updatedTraineeDu);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _duRepositoryMock.Verify(x => x.AddAsync(It.Is<Du>(d => d.Name == "NewDU")), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdatesAllFields()
        {
            // Arrange
            var command = new UpdateTraineeDuCommand
            {
                TraineeDuId = 1,
                DuAllocated = "DU3",
                Location = "Kolkata",
                OjtMentor = "Ms. Banerjee"
            };

            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1, DuId = 1 };
            var trainee = new Trainee { Id = 1, User = new User { Username = "John" } };
            var du = new Du { Id = 3, Name = "DU3" };
            var updatedTraineeDu = new TraineeDu
            {
                Id = 1,
                TraineeId = 1,
                DuId = 3,
                Location = "Kolkata",
                ojtMenter = "Ms. Banerjee",
                Trainee = trainee,
                Du = du
            };

            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(command.TraineeDuId)).ReturnsAsync(existingTraineeDu);
            _duRepositoryMock.Setup(x => x.GetByNameAsync(command.DuAllocated)).ReturnsAsync(du);
            _traineeDuRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TraineeDu>())).ReturnsAsync(updatedTraineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdAsync(updatedTraineeDu.Id)).ReturnsAsync(updatedTraineeDu);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _traineeDuRepositoryMock.Verify(x => x.UpdateAsync(It.Is<TraineeDu>(
                td => td.Location == "Kolkata" && td.ojtMenter == "Ms. Banerjee" && td.DuId == 3)), Times.Once);
        }
    }
}
