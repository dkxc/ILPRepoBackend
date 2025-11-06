using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.PhaseTypes;
using IlpRepoBackend.Application.Query.PhaseTypes;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.PhaseTypes
{
    public class GetPhaseTypeByIdHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPhaseTypeByIdHandler _handler;

        public GetPhaseTypeByIdHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetPhaseTypeByIdHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidPhaseTypeId_ReturnsPhaseType()
        {
            // Arrange
            var phaseTypeId = 1;
            var phaseType = new PhaseType
            {
                Id = phaseTypeId,
                Name = "Foundation Phase",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var phaseTypeDto = new PhaseTypeDto
            {
                Id = phaseTypeId,
                Name = "Foundation Phase"
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(phaseTypeId))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(phaseType))
                .Returns(phaseTypeDto);

            var query = new GetPhaseTypeByIdQuery { Id = phaseTypeId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(phaseTypeId);
            result.Data.Name.ShouldBe("Foundation Phase");
        }

        [Fact]
        public async Task Handle_PhaseTypeNotFound_ReturnsFailure()
        {
            // Arrange
            var phaseTypeId = 999;
            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(phaseTypeId))
                .ReturnsAsync((PhaseType?)null);

            var query = new GetPhaseTypeByIdQuery { Id = phaseTypeId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("999");
            result.Message.ShouldContain("not found");
            result.Data.ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task Handle_DifferentPhaseTypeIds_CallsRepositoryWithCorrectId(int phaseTypeId)
        {
            // Arrange
            var phaseType = new PhaseType { Id = phaseTypeId, Name = "Test Phase" };
            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(phaseTypeId))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(It.IsAny<PhaseType>()))
                .Returns(new PhaseTypeDto { Id = phaseTypeId });

            var query = new GetPhaseTypeByIdQuery { Id = phaseTypeId };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetPhaseTypeByIdAsync(phaseTypeId), Times.Once);
        }

        [Theory]
        [InlineData("Foundation Phase")]
        [InlineData("Advanced Phase")]
        [InlineData("Specialization Phase")]
        public async Task Handle_PhaseTypesWithDifferentNames_ReturnsCorrectly(string name)
        {
            // Arrange
            var phaseType = new PhaseType
            {
                Id = 1,
                Name = name
            };

            var phaseTypeDto = new PhaseTypeDto
            {
                Id = 1,
                Name = name
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(1))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(phaseType))
                .Returns(phaseTypeDto);

            var query = new GetPhaseTypeByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Name.ShouldBe(name);
        }

        [Fact]
        public async Task Handle_MapsPhaseTypeCorrectly()
        {
            // Arrange
            var phaseType = new PhaseType
            {
                Id = 1,
                Name = "Test Phase",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var phaseTypeDto = new PhaseTypeDto
            {
                Id = 1,
                Name = "Test Phase"
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(1))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(phaseType))
                .Returns(phaseTypeDto);

            var query = new GetPhaseTypeByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mapperMock.Verify(x => x.Map<PhaseTypeDto>(phaseType), Times.Once);
            result.Data.ShouldBe(phaseTypeDto);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResponse()
        {
            // Arrange
            var phaseType = new PhaseType { Id = 1, Name = "Test Phase" };
            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(1))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(It.IsAny<PhaseType>()))
                .Returns(new PhaseTypeDto());

            var query = new GetPhaseTypeByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_InvalidId_StillCallsRepository()
        {
            // Arrange
            var invalidId = 0;
            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(invalidId))
                .ReturnsAsync((PhaseType?)null);

            var query = new GetPhaseTypeByIdQuery { Id = invalidId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            _batchRepositoryMock.Verify(x => x.GetPhaseTypeByIdAsync(invalidId), Times.Once);
        }

        [Fact]
        public async Task Handle_NegativeId_StillCallsRepository()
        {
            // Arrange
            var negativeId = -1;
            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(negativeId))
                .ReturnsAsync((PhaseType?)null);

            var query = new GetPhaseTypeByIdQuery { Id = negativeId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            _batchRepositoryMock.Verify(x => x.GetPhaseTypeByIdAsync(negativeId), Times.Once);
        }

        [Fact]
        public async Task Handle_WithLongPhaseName_HandlesCorrectly()
        {
            // Arrange
            var phaseType = new PhaseType
            {
                Id = 1,
                Name = "Advanced Full Stack Development and Specialization Phase with Extended Learning Modules"
            };

            var phaseTypeDto = new PhaseTypeDto
            {
                Id = 1,
                Name = phaseType.Name
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypeByIdAsync(1))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(phaseType))
                .Returns(phaseTypeDto);

            var query = new GetPhaseTypeByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Name.Length.ShouldBeGreaterThan(50);
        }
    }
}
