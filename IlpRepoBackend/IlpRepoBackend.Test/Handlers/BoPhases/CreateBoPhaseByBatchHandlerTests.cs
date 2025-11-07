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
    public class CreateBoPhaseByBatchHandlerTests
    {
        private readonly Mock<IBoPhaseRepository> _boPhaseRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IBuddyRepository> _buddyRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IDuRepository> _duRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateBoPhaseByBatchHandler _handler;

        public CreateBoPhaseByBatchHandlerTests()
        {
            _boPhaseRepositoryMock = new Mock<IBoPhaseRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _buddyRepositoryMock = new Mock<IBuddyRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _duRepositoryMock = new Mock<IDuRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateBoPhaseByBatchHandler(
                _boPhaseRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _buddyRepositoryMock.Object,
                _userRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _duRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesMultipleBoPhases()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto
                    {
                        TraineeName = "John Doe",
                        Email = "john@example.com",
                        BuddyName = "Jane Smith",
                        DuName = "DU1"
                    },
                    new AddBoPhaseForBatchDto
                    {
                        TraineeName = "Alice Johnson",
                        Email = "alice@example.com",
                        BuddyName = "Bob Wilson",
                        DuName = "DU2"
                    }
                }
            };

            var batch = new Batch { Id = batchId };
            var user1 = new User { Id = 1, Username = "John Doe", Email = "john@example.com" };
            var user2 = new User { Id = 2, Username = "Alice Johnson", Email = "alice@example.com" };
            var trainee1 = new Trainee { Id = 1, UserId = 1, BatchId = batchId, User = user1 };
            var trainee2 = new Trainee { Id = 2, UserId = 2, BatchId = batchId, User = user2 };
            var du1 = new Du { Id = 1, Name = "DU1" };
            var du2 = new Du { Id = 2, Name = "DU2" };
            var buddy1 = new Buddy { Id = 1, Name = "Jane Smith", DuId = 1, Du = du1 };
            var buddy2 = new Buddy { Id = 2, Name = "Bob Wilson", DuId = 2, Du = du2 };
            var boPhase1 = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee1, Buddy = buddy1 };
            var boPhase2 = new BoPhase { Id = 2, TraineeId = 2, BuddyId = 2, Trainee = trainee2, Buddy = buddy2 };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user1);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("alice@example.com")).ReturnsAsync(user2);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee1);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(2)).ReturnsAsync(trainee2);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(It.IsAny<int>())).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync("DU1")).ReturnsAsync(du1);
            _duRepositoryMock.Setup(x => x.GetByNameAsync("DU2")).ReturnsAsync(du2);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync("Jane Smith")).ReturnsAsync(buddy1);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync("Bob Wilson")).ReturnsAsync(buddy2);
            _boPhaseRepositoryMock.SetupSequence(x => x.AddAsync(It.IsAny<BoPhase>()))
                .ReturnsAsync(boPhase1)
                .ReturnsAsync(boPhase2);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<BoPhase> { boPhase1, boPhase2 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            _boPhaseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BoPhase>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_NullBoPhasesList_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = 1,
                BoPhases = null
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No BO Phase records");
        }

        [Fact]
        public async Task Handle_EmptyBoPhasesList_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = 1,
                BoPhases = new List<AddBoPhaseForBatchDto>()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No BO Phase records");
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = 999,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "John", Email = "john@example.com", BuddyName = "Jane" }
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
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "", Email = "john@example.com", BuddyName = "Jane" }
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
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "John", Email = "", BuddyName = "Jane" }
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
        public async Task Handle_EmptyBuddyName_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "John", Email = "john@example.com", BuddyName = "" }
                }
            };

            var batch = new Batch { Id = batchId };
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Buddy name cannot be empty");
        }

        [Fact]
        public async Task Handle_TraineeNotBelongToBatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "John", Email = "john@example.com", BuddyName = "Jane" }
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
        public async Task Handle_TraineeAlreadyHasBoPhase_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "John", Email = "john@example.com", BuddyName = "Jane" }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, BatchId = batchId };
            var existingBoPhase = new BoPhase { Id = 1, TraineeId = 1 };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(1))
                .ReturnsAsync(new List<BoPhase> { existingBoPhase });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already has a BO Phase assignment");
        }

        [Fact]
        public async Task Handle_TraineeNameMismatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto { TraineeName = "John", Email = "jane@example.com", BuddyName = "Buddy" }
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
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto
                    {
                        TraineeName = "John",
                        Email = "john@example.com",
                        BuddyName = "Jane",
                        DuName = "NewDU"
                    }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, BatchId = batchId, User = user };
            var newDu = new Du { Id = 1, Name = "NewDU" };
            var buddy = new Buddy { Id = 1, Name = "Jane", DuId = 1, Du = newDu };
            var boPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = buddy };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(1)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync("NewDU")).ReturnsAsync((Du)null);
            _duRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Du>())).ReturnsAsync(newDu);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync("Jane")).ReturnsAsync(buddy);
            _boPhaseRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BoPhase>())).ReturnsAsync(boPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<BoPhase> { boPhase });

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
            var batchId = 1;
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = new List<AddBoPhaseForBatchDto>
                {
                    new AddBoPhaseForBatchDto
                    {
                        TraineeName = "John",
                        Email = "john@example.com",
                        BuddyName = "NewBuddy",
                        DuName = "DU1"
                    }
                }
            };

            var batch = new Batch { Id = batchId };
            var user = new User { Id = 1, Username = "John", Email = "john@example.com" };
            var trainee = new Trainee { Id = 1, UserId = 1, BatchId = batchId, User = user };
            var du = new Du { Id = 1, Name = "DU1" };
            var newBuddy = new Buddy { Id = 1, Name = "NewBuddy", DuId = 1, Du = du };
            var boPhase = new BoPhase { Id = 1, TraineeId = 1, BuddyId = 1, Trainee = trainee, Buddy = newBuddy };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId)).ReturnsAsync(batch);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
            _traineeRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(trainee);
            _boPhaseRepositoryMock.Setup(x => x.GetByTraineeIdAsync(1)).ReturnsAsync(new List<BoPhase>());
            _duRepositoryMock.Setup(x => x.GetByNameAsync("DU1")).ReturnsAsync(du);
            _buddyRepositoryMock.Setup(x => x.GetByNameAsync("NewBuddy")).ReturnsAsync((Buddy)null);
            _buddyRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Buddy>())).ReturnsAsync(newBuddy);
            _boPhaseRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BoPhase>())).ReturnsAsync(boPhase);
            _boPhaseRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<BoPhase> { boPhase });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _buddyRepositoryMock.Verify(x => x.AddAsync(It.Is<Buddy>(b => b.Name == "NewBuddy")), Times.Once);
        }
    }
}
