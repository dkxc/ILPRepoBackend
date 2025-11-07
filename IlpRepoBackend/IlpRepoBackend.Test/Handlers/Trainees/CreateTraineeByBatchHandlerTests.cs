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
    public class CreateTraineeByBatchHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly CreateTraineeByBatchHandler _handler;

        public CreateTraineeByBatchHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _handler = new CreateTraineeByBatchHandler(
                _mapperMock.Object,
                _userRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _batchRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidCommandWithMultipleTrainees_CreatesAllTrainees()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch { Id = batchId, BatchName = "Batch 1" };
            var trainees = new List<AddTraineeForABatchDto>
            {
                new AddTraineeForABatchDto
                {
                    Username = "trainee1",
                    Email = "trainee1@example.com",
                    Password = "Pass123!",
                    Status = TraineeStatus.Active
                },
                new AddTraineeForABatchDto
                {
                    Username = "trainee2",
                    Email = "trainee2@example.com",
                    Password = "Pass123!",
                    Status = TraineeStatus.Active
                }
            };

            var command = new CreateTraineeByBatch
            {
                BatchId = batchId,
                Trainees = trainees
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _traineeRepositoryMock.Setup(x => x.AadhaarIdExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => new User { Id = 1, Username = u.Username });
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>()))
                .ReturnsAsync((Trainee t) => new Trainee { Id = 1, Email = t.Email });
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>()))
                .Returns((Trainee t) => new TraineeDto { Email = t.Email });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Exactly(2));
            _traineeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Trainee>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_EmptyTraineesList_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = new List<AddTraineeForABatchDto>()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No trainees to add");
        }

        [Fact]
        public async Task Handle_NullTraineesList_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = null
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No trainees to add");
        }

        [Fact]
        public async Task Handle_BatchDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeByBatch
            {
                BatchId = 999,
                Trainees = new List<AddTraineeForABatchDto>
                {
                    new AddTraineeForABatchDto { Username = "test", Email = "test@example.com", Password = "Pass123!" }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Batch?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Batch does not exist");
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ReturnsFailureAndStopsProcessing()
        {
            // Arrange
            var batch = new Batch { Id = 1, BatchName = "Batch 1" };
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = new List<AddTraineeForABatchDto>
                {
                    new AddTraineeForABatchDto { Username = "trainee1", Email = "existing@example.com", Password = "Pass123!" },
                    new AddTraineeForABatchDto { Username = "trainee2", Email = "new@example.com", Password = "Pass123!" }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync("existing@example.com")).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Email");
            result.Message.ShouldContain("existing@example.com");
            result.Message.ShouldContain("already exists");
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateAadhaarId_ReturnsFailure()
        {
            // Arrange
            var batch = new Batch { Id = 1, BatchName = "Batch 1" };
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = new List<AddTraineeForABatchDto>
                {
                    new AddTraineeForABatchDto
                    {
                        Username = "trainee1",
                        Email = "trainee1@example.com",
                        Password = "Pass123!",
                        AadhaarId = "123456789012"
                    }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _traineeRepositoryMock.Setup(x => x.AadhaarIdExistsAsync("123456789012")).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Aadhaar ID");
            result.Message.ShouldContain("already exists");
        }

        [Fact]
        public async Task Handle_IncludesBatchNameInResponse()
        {
            // Arrange
            var batch = new Batch { Id = 1, BatchName = "Test Batch" };
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = new List<AddTraineeForABatchDto>
                {
                    new AddTraineeForABatchDto { Username = "trainee1", Email = "trainee1@example.com", Password = "Pass123!" }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(new User { Id = 1, Username = "trainee1" });
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>()))
                .ReturnsAsync(new Trainee { Id = 1 });
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>()))
                .Returns(new TraineeDto());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].BatchName.ShouldBe("Test Batch");
        }

        [Fact]
        public async Task Handle_ExceptionDuringProcessing_ReturnsFailure()
        {
            // Arrange
            var batch = new Batch { Id = 1, BatchName = "Batch 1" };
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = new List<AddTraineeForABatchDto>
                {
                    new AddTraineeForABatchDto { Username = "trainee1", Email = "trainee1@example.com", Password = "Pass123!" }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error creating trainees");
        }

        [Fact]
        public async Task Handle_HashesPasswordsForAllTrainees()
        {
            // Arrange
            var batch = new Batch { Id = 1, BatchName = "Batch 1" };
            var command = new CreateTraineeByBatch
            {
                BatchId = 1,
                Trainees = new List<AddTraineeForABatchDto>
                {
                    new AddTraineeForABatchDto { Username = "trainee1", Email = "trainee1@example.com", Password = "Pass123!" },
                    new AddTraineeForABatchDto { Username = "trainee2", Email = "trainee2@example.com", Password = "Pass456!" }
                }
            };

            var capturedUsers = new List<User>();
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUsers.Add(u))
                .ReturnsAsync((User u) => new User { Id = capturedUsers.Count, Username = u.Username });
            _traineeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Trainee>()))
                .ReturnsAsync(new Trainee { Id = 1 });
            _mapperMock.Setup(x => x.Map<TraineeDto>(It.IsAny<Trainee>()))
                .Returns(new TraineeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUsers.Count.ShouldBe(2);
            capturedUsers.All(u => u.PasswordHash != "Pass123!" && u.PasswordHash != "Pass456!").ShouldBeTrue();
            capturedUsers.All(u => !string.IsNullOrEmpty(u.PasswordHash)).ShouldBeTrue();
        }
    }
}
