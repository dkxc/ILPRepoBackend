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
    public class GetTraineesByBatchIdHandlerTests
    {
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetTraineesByBatchIdHandler _handler;

        public GetTraineesByBatchIdHandlerTests()
        {
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetTraineesByBatchIdHandler(_traineeRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchId_ReturnsTrainees()
        {
            // Arrange
            var batchId = 1;
            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, Email = "trainee1@example.com" },
                new Trainee { Id = 2, BatchId = batchId, Email = "trainee2@example.com" }
            };

            var traineeDtos = new List<TraineeDto>
            {
                new TraineeDto { Id = 1, BatchId = batchId },
                new TraineeDto { Id = 2, BatchId = batchId }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _mapperMock.Setup(x => x.Map<List<TraineeDto>>(trainees)).Returns(traineeDtos);

            var query = new GetTraineesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            result.Data.All(t => t.BatchId == batchId).ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_BatchWithNoTrainees_ReturnsEmptyList()
        {
            // Arrange
            var batchId = 999;
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());
            _mapperMock.Setup(x => x.Map<List<TraineeDto>>(It.IsAny<List<Trainee>>()))
                .Returns(new List<TraineeDto>());

            var query = new GetTraineesByBatchIdQuery(batchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Handle_DifferentBatchIds_CallsRepositoryWithCorrectId(int batchId)
        {
            // Arrange
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());
            _mapperMock.Setup(x => x.Map<List<TraineeDto>>(It.IsAny<List<Trainee>>()))
                .Returns(new List<TraineeDto>());

            var query = new GetTraineesByBatchIdQuery(batchId);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _traineeRepositoryMock.Verify(x => x.GetByBatchIdAsync(batchId), Times.Once);
        }
    }
}
