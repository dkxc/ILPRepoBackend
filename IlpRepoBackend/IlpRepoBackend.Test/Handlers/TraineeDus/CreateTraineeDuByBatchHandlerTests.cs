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
    public class CreateTraineeDuByBatchHandlerTests
    {
        private readonly Mock<ITraineeDuRepository> _traineeDuRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IDuRepository> _duRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateTraineeDuByBatchHandler _handler;

        public CreateTraineeDuByBatchHandlerTests()
        {
            _traineeDuRepositoryMock = new Mock<ITraineeDuRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _duRepositoryMock = new Mock<IDuRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateTraineeDuByBatchHandler(
                _traineeDuRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _duRepositoryMock.Object,
                _userRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesMultipleTraineeDus()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto
                    {
                        TraineeName = "John Doe",
                        Email = "john@example.com",
                        DuName = "DU1",
                        Location = "Bangalore",
                        OjtMenter = "Mr. Smith"
                    },
                    new AddTraineeDuForBatchDto
                    {
                        TraineeName = "Alice Johnson",
                        Email = "alice@example.com",
                        DuName = "DU2",
                        Location = "Mumbai",
                        OjtMenter = "Ms. Kumar"
                    }
                }
            };

            var batch = new Batch { Id = batchId };
            var user1 = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var user2 = new User { Id = 2, Username = "Alice Johnson", Email = "alice@example.com" };
            var trainee1 = new Trainee { Id = 1, UserId = 1, BatchId = batchId };
            var trainee2 = new Trainee { Id = 2, UserId = 2, BatchId = batchId };
            var du1 = new Du { Id = 1, Name = "DU1" };
            var du2 = new Du { Id = 2, Name = "DU2" };
            var traineeDu1 = new TraineeDu { Id = 1, TraineeId = 1, DuId = 1 };
            var traineeDu2 = new TraineeDu { Id = 2, TraineeId = 2, DuId = 2 };
            var traineeDuDtos = new List<TraineeDuDto>
            {
                new TraineeDuDto { Id = 1, TraineeName = "John Doe" },
                new TraineeDuDto { Id = 2, TraineeName = "Alice Johnson" }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user1);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("alice@example.com")).ReturnsAsync(user2);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee1);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(2)).ReturnsAsync(trainee2);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(It.IsAny<int>())).ReturnsAsync(new List<TraineeDu>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync("DU1")).ReturnsAsync(du1);
            _duRepositoryMock.Setup(x => x.GetByNameAsync("DU2")).ReturnsAsync(du2);
            _traineeDuRepositoryMock.SetupSequence(x => x.AddAsync(It.IsAny<TraineeDu>()))
                .ReturnsAsync(traineeDu1)
                .ReturnsAsync(traineeDu2);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<TraineeDu> { traineeDu1, traineeDu2 });
            _mapperMock.Setup(x => x.Map<List<TraineeDuDto>>(It.IsAny<List<TraineeDu>>()))
                .Returns(traineeDuDtos);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            _traineeDuRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TraineeDu>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_NullTraineeDusList_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = 1,
                TraineeDus = null
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No TraineeDu records");
        }

        [Fact]
        public async Task Handle_EmptyTraineeDusList_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = 1,
                TraineeDus = new List<AddTraineeDuForBatchDto>()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No TraineeDu records");
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = 999,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "John", Email = "john@example.com", DuName = "DU1" }
                }
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Batch)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Batch does not exist");
        }

        [Fact]
        public async Task Handle_EmptyTraineeName_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "", Email = "john@example.com", DuName = "DU1" }
                }
            };

            var batch = new Batch { Id = batchId };
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);

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
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "John", Email = "", DuName = "DU1" }
                }
            };

            var batch = new Batch { Id = batchId };
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);

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
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "John", Email = "john@example.com", DuName = "" }
                }
            };

            var batch = new Batch { Id = batchId };
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("DU name cannot be empty");
        }

        [Fact]
        public async Task Handle_TraineeNotBelongToBatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "John", Email = "john@example.com", DuName = "DU1" }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, BatchId = 2 }; // Different batch

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("does not belong to the specified batch");
        }

        [Fact]
        public async Task Handle_TraineeAlreadyHasDuAssignment_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "John", Email = "john@example.com", DuName = "DU1" }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, BatchId = batchId };
            var existingTraineeDu = new TraineeDu { Id = 1, TraineeId = 1 };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(1))
                .ReturnsAsync(new List<TraineeDu> { existingTraineeDu });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already has a DU assignment");
        }

        [Fact]
        public async Task Handle_TraineeNameMismatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto { TraineeName = "John", Email = "jane@example.com", DuName = "DU1" }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "Jane", Email = "jane@example.com" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("jane@example.com")).ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("does not match");
        }

        [Fact]
        public async Task Handle_CreatesDuIfNotExists()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = new List<AddTraineeDuForBatchDto>
                {
                    new AddTraineeDuForBatchDto
                    {
                        TraineeName = "John",
                        Email = "john@example.com",
                        DuName = "NewDU",
                        Location = "Pune",
                        OjtMenter = "Mr. Patel"
                    }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, BatchId = batchId };
            var newDu = new Du { Id = 1, Name = "NewDU" };
            var traineeDu = new TraineeDu { Id = 1, TraineeId = 1, DuId = 1 };
            var traineeDuDtos = new List<TraineeDuDto> { new TraineeDuDto { Id = 1 } };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee);
            _traineeDuRepositoryMock.Setup(x => x.GetByTraineeIdAsync(1)).ReturnsAsync(new List<TraineeDu>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync("NewDU")).ReturnsAsync((Du)null);
            _duRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Du>())).ReturnsAsync(newDu);
            _traineeDuRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TraineeDu>())).ReturnsAsync(traineeDu);
            _traineeDuRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<TraineeDu> { traineeDu });
            _mapperMock.Setup(x => x.Map<List<TraineeDuDto>>(It.IsAny<List<TraineeDu>>()))
                .Returns(traineeDuDtos);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _duRepositoryMock.Verify(x => x.AddAsync(It.Is<Du>(d => d.Name == "NewDU")), Times.Once);
        }
    }
}
