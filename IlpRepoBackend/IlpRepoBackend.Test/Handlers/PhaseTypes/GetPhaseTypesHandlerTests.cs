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
    public class GetPhaseTypesHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPhaseTypesHandler _handler;

        public GetPhaseTypesHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetPhaseTypesHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_PhaseTypesExist_ReturnsAllPhaseTypes()
        {
            // Arrange
            var phaseTypes = new List<PhaseType>
            {
                new PhaseType { Id = 1, Name = "Foundation Phase", CreatedAt = DateTime.UtcNow },
                new PhaseType { Id = 2, Name = "Advanced Phase", CreatedAt = DateTime.UtcNow },
                new PhaseType { Id = 3, Name = "Specialization Phase", CreatedAt = DateTime.UtcNow }
            };

            var phaseTypeDtos = new List<PhaseTypeDto>
            {
                new PhaseTypeDto { Id = 1, Name = "Foundation Phase" },
                new PhaseTypeDto { Id = 2, Name = "Advanced Phase" },
                new PhaseTypeDto { Id = 3, Name = "Specialization Phase" }
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(phaseTypes);
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(phaseTypes))
                .Returns(phaseTypeDtos);

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(3);
            result.Data[0].Name.ShouldBe("Foundation Phase");
            result.Data[1].Name.ShouldBe("Advanced Phase");
            result.Data[2].Name.ShouldBe("Specialization Phase");
        }

        [Fact]
        public async Task Handle_NoPhaseTypes_ReturnsEmptyList()
        {
            // Arrange
            var phaseTypes = new List<PhaseType>();
            var phaseTypeDtos = new List<PhaseTypeDto>();

            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(phaseTypes);
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(phaseTypes))
                .Returns(phaseTypeDtos);

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_SinglePhaseType_ReturnsSingle()
        {
            // Arrange
            var phaseTypes = new List<PhaseType>
            {
                new PhaseType { Id = 1, Name = "Training Phase" }
            };

            var phaseTypeDtos = new List<PhaseTypeDto>
            {
                new PhaseTypeDto { Id = 1, Name = "Training Phase" }
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(phaseTypes);
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(phaseTypes))
                .Returns(phaseTypeDtos);

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].Name.ShouldBe("Training Phase");
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(new List<PhaseType>());
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(It.IsAny<List<PhaseType>>()))
                .Returns(new List<PhaseTypeDto>());

            var query = new GetPhaseTypesQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetPhaseTypesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_MapsCorrectly()
        {
            // Arrange
            var phaseTypes = new List<PhaseType>
            {
                new PhaseType { Id = 1, Name = "Phase 1" },
                new PhaseType { Id = 2, Name = "Phase 2" }
            };

            var phaseTypeDtos = new List<PhaseTypeDto>
            {
                new PhaseTypeDto { Id = 1, Name = "Phase 1" },
                new PhaseTypeDto { Id = 2, Name = "Phase 2" }
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(phaseTypes);
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(phaseTypes))
                .Returns(phaseTypeDtos);

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mapperMock.Verify(x => x.Map<List<PhaseTypeDto>>(phaseTypes), Times.Once);
            result.Data.ShouldBe(phaseTypeDtos);
        }

        [Fact]
        public async Task Handle_WithMultiplePhaseTypes_ReturnsInOrder()
        {
            // Arrange
            var phaseTypes = new List<PhaseType>
            {
                new PhaseType { Id = 5, Name = "Phase E" },
                new PhaseType { Id = 3, Name = "Phase C" },
                new PhaseType { Id = 1, Name = "Phase A" },
                new PhaseType { Id = 4, Name = "Phase D" },
                new PhaseType { Id = 2, Name = "Phase B" }
            };

            var phaseTypeDtos = new List<PhaseTypeDto>
            {
                new PhaseTypeDto { Id = 5, Name = "Phase E" },
                new PhaseTypeDto { Id = 3, Name = "Phase C" },
                new PhaseTypeDto { Id = 1, Name = "Phase A" },
                new PhaseTypeDto { Id = 4, Name = "Phase D" },
                new PhaseTypeDto { Id = 2, Name = "Phase B" }
            };

            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(phaseTypes);
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(phaseTypes))
                .Returns(phaseTypeDtos);

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Count.ShouldBe(5);
            result.Data[0].Name.ShouldBe("Phase E");
            result.Data[2].Name.ShouldBe("Phase A");
            result.Data[4].Name.ShouldBe("Phase B");
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResponse()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(new List<PhaseType>());
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(It.IsAny<List<PhaseType>>()))
                .Returns(new List<PhaseTypeDto>());

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_WithVariousPhaseNames_HandlesCorrectly()
        {
            // Arrange
            var phaseTypes = new List<PhaseType>
            {
                new PhaseType { Id = 1, Name = "Foundation and Core Concepts" },
                new PhaseType { Id = 2, Name = "Advanced Topics and Projects" },
                new PhaseType { Id = 3, Name = "Specialization Track" }
            };

            var phaseTypeDtos = phaseTypes.Select(pt => new PhaseTypeDto
            {
                Id = pt.Id,
                Name = pt.Name
            }).ToList();

            _batchRepositoryMock.Setup(x => x.GetPhaseTypesAsync())
                .ReturnsAsync(phaseTypes);
            _mapperMock.Setup(x => x.Map<List<PhaseTypeDto>>(phaseTypes))
                .Returns(phaseTypeDtos);

            var query = new GetPhaseTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            result.Data.All(pt => !string.IsNullOrEmpty(pt.Name)).ShouldBeTrue();
        }
    }
}
