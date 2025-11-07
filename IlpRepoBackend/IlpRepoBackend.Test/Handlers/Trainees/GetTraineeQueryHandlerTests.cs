using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Trainees;
using IlpRepoBackend.Application.Query.Trainees;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Trainees
{
    public class GetTraineeQueryHandlerTests
    {
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetTraineeQueryHandler _handler;

        public GetTraineeQueryHandlerTests()
        {
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetTraineeQueryHandler(_traineeRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_TraineesExist_ReturnsAllTrainees()
        {
            // Arrange
            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "trainee1@example.com", Status = TraineeStatus.Active },
                new Trainee { Id = 2, Email = "trainee2@example.com", Status = TraineeStatus.Active },
                new Trainee { Id = 3, Email = "trainee3@example.com", Status = TraineeStatus.Inactive }
            };

            var traineeDtos = new List<TraineeDto>
            {
                new TraineeDto { Id = 1, Email = "trainee1@example.com" },
                new TraineeDto { Id = 2, Email = "trainee2@example.com" },
                new TraineeDto { Id = 3, Email = "trainee3@example.com" }
            };

            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(trainees);
            _mapperMock.Setup(x => x.Map<List<TraineeDto>>(trainees)).Returns(traineeDtos);

            var query = new GetTraineeQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
        }

        [Fact]
        public async Task Handle_NoTrainees_ReturnsEmptyList()
        {
            // Arrange
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Trainee>());
            _mapperMock.Setup(x => x.Map<List<TraineeDto>>(It.IsAny<List<Trainee>>()))
                .Returns(new List<TraineeDto>());

            var query = new GetTraineeQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_RepositoryReturnsNull_ReturnsFailure()
        {
            // Arrange
            _traineeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync((List<Trainee>?)null);

            var query = new GetTraineeQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to retrieve trainees");
        }
    }
}
