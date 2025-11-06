using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.BatchTypes;
using IlpRepoBackend.Application.Query.BatchTypes;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.BatchTypes
{
    public class GetBatchTypesHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetBatchTypesHandler _handler;

        public GetBatchTypesHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetBatchTypesHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_BatchTypesExist_ReturnsAllBatchTypes()
        {
            // Arrange
            var batchTypes = new List<BatchType>
            {
                new BatchType { Id = 1, Name = "Full Stack Development", CreatedAt = DateTime.UtcNow },
                new BatchType { Id = 2, Name = "Data Science", CreatedAt = DateTime.UtcNow },
                new BatchType { Id = 3, Name = "DevOps Engineering", CreatedAt = DateTime.UtcNow }
            };

            var batchTypeDtos = new List<BatchTypeDto>
            {
                new BatchTypeDto { Id = 1, Name = "Full Stack Development" },
                new BatchTypeDto { Id = 2, Name = "Data Science" },
                new BatchTypeDto { Id = 3, Name = "DevOps Engineering" }
            };

            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(batchTypes);
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(batchTypes))
                .Returns(batchTypeDtos);

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(3);
            result.Data[0].Name.ShouldBe("Full Stack Development");
            result.Data[1].Name.ShouldBe("Data Science");
            result.Data[2].Name.ShouldBe("DevOps Engineering");
        }

        [Fact]
        public async Task Handle_NoBatchTypes_ReturnsEmptyList()
        {
            // Arrange
            var batchTypes = new List<BatchType>();
            var batchTypeDtos = new List<BatchTypeDto>();

            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(batchTypes);
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(batchTypes))
                .Returns(batchTypeDtos);

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_SingleBatchType_ReturnsSingle()
        {
            // Arrange
            var batchTypes = new List<BatchType>
            {
                new BatchType { Id = 1, Name = "Cloud Architecture" }
            };

            var batchTypeDtos = new List<BatchTypeDto>
            {
                new BatchTypeDto { Id = 1, Name = "Cloud Architecture" }
            };

            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(batchTypes);
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(batchTypes))
                .Returns(batchTypeDtos);

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].Name.ShouldBe("Cloud Architecture");
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(new List<BatchType>());
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(It.IsAny<List<BatchType>>()))
                .Returns(new List<BatchTypeDto>());

            var query = new GetBatchTypesQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.GetBatchTypesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_MapsCorrectly()
        {
            // Arrange
            var batchTypes = new List<BatchType>
            {
                new BatchType { Id = 1, Name = "Test Type 1" },
                new BatchType { Id = 2, Name = "Test Type 2" }
            };

            var batchTypeDtos = new List<BatchTypeDto>
            {
                new BatchTypeDto { Id = 1, Name = "Test Type 1" },
                new BatchTypeDto { Id = 2, Name = "Test Type 2" }
            };

            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(batchTypes);
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(batchTypes))
                .Returns(batchTypeDtos);

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mapperMock.Verify(x => x.Map<List<BatchTypeDto>>(batchTypes), Times.Once);
            result.Data.ShouldBe(batchTypeDtos);
        }

        [Fact]
        public async Task Handle_WithMultipleBatchTypes_ReturnsInOrder()
        {
            // Arrange
            var batchTypes = new List<BatchType>
            {
                new BatchType { Id = 5, Name = "Type E" },
                new BatchType { Id = 3, Name = "Type C" },
                new BatchType { Id = 1, Name = "Type A" },
                new BatchType { Id = 4, Name = "Type D" },
                new BatchType { Id = 2, Name = "Type B" }
            };

            var batchTypeDtos = new List<BatchTypeDto>
            {
                new BatchTypeDto { Id = 5, Name = "Type E" },
                new BatchTypeDto { Id = 3, Name = "Type C" },
                new BatchTypeDto { Id = 1, Name = "Type A" },
                new BatchTypeDto { Id = 4, Name = "Type D" },
                new BatchTypeDto { Id = 2, Name = "Type B" }
            };

            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(batchTypes);
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(batchTypes))
                .Returns(batchTypeDtos);

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Count.ShouldBe(5);
            result.Data[0].Name.ShouldBe("Type E");
            result.Data[2].Name.ShouldBe("Type A");
            result.Data[4].Name.ShouldBe("Type B");
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResponse()
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(new List<BatchType>());
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(It.IsAny<List<BatchType>>()))
                .Returns(new List<BatchTypeDto>());

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_WithLongNames_HandlesCorrectly()
        {
            // Arrange
            var batchTypes = new List<BatchType>
            {
                new BatchType 
                { 
                    Id = 1, 
                    Name = "Full Stack Web Development with React, Node.js, and MongoDB" 
                },
                new BatchType 
                { 
                    Id = 2, 
                    Name = "Advanced Data Science and Machine Learning with Python and TensorFlow" 
                }
            };

            var batchTypeDtos = batchTypes.Select(bt => new BatchTypeDto 
            { 
                Id = bt.Id, 
                Name = bt.Name 
            }).ToList();

            _batchRepositoryMock.Setup(x => x.GetBatchTypesAsync())
                .ReturnsAsync(batchTypes);
            _mapperMock.Setup(x => x.Map<List<BatchTypeDto>>(batchTypes))
                .Returns(batchTypeDtos);

            var query = new GetBatchTypesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(2);
            result.Data[0].Name.Length.ShouldBeGreaterThan(50);
        }
    }
}
