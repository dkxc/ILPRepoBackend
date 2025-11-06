using AutoMapper;
using IlpRepoBackend.Application.Command.BatchTypes;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.BatchTypes;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.BatchTypes
{
    public class UpdateBatchTypeHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateBatchTypeHandler _handler;

        public UpdateBatchTypeHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateBatchTypeHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesBatchType()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = "Updated Batch Type"
            };

            var batchType = new BatchType
            {
                Id = 1,
                Name = command.Name,
                UpdatedAt = DateTime.UtcNow
            };

            var batchTypeDto = new BatchTypeDto
            {
                Id = 1,
                Name = command.Name
            };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()))
                .ReturnsAsync(batchType);
            _mapperMock.Setup(x => x.Map<BatchTypeDto>(batchType))
                .Returns(batchTypeDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(1);
            result.Data.Name.ShouldBe("Updated Batch Type");
            result.Message.ShouldContain("successfully");
            _batchRepositoryMock.Verify(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 0,
                Name = "Test"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid id");
            _batchRepositoryMock.Verify(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NegativeId_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = -1,
                Name = "Test"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid id");
        }

        [Fact]
        public async Task Handle_EmptyName_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = ""
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Name is required");
        }

        [Fact]
        public async Task Handle_NullName_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = null
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("required");
        }

        [Fact]
        public async Task Handle_WhitespaceName_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = "   "
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("required");
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = "Existing Batch Type"
            };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
            _batchRepositoryMock.Verify(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BatchTypeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 999,
                Name = "Test Batch Type"
            };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()))
                .ReturnsAsync((BatchType?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Theory]
        [InlineData("Full Stack Development")]
        [InlineData("Data Science")]
        [InlineData("DevOps Engineering")]
        public async Task Handle_DifferentNames_UpdatesCorrectly(string name)
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = name
            };

            var batchType = new BatchType { Id = 1, Name = name };
            var batchTypeDto = new BatchTypeDto { Id = 1, Name = name };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(name, 1))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()))
                .ReturnsAsync(batchType);
            _mapperMock.Setup(x => x.Map<BatchTypeDto>(It.IsAny<BatchType>()))
                .Returns(batchTypeDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Name.ShouldBe(name);
        }

        [Fact]
        public async Task Handle_ExcludesCurrentIdFromDuplicateCheck()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = "Test Batch Type"
            };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()))
                .ReturnsAsync(new BatchType { Id = 1, Name = command.Name });
            _mapperMock.Setup(x => x.Map<BatchTypeDto>(It.IsAny<BatchType>()))
                .Returns(new BatchTypeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(
                x => x.BatchTypeNameExistsAsync(command.Name, command.Id),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_UpdatesTimestamp()
        {
            // Arrange
            var command = new UpdateBatchTypeCommand
            {
                Id = 1,
                Name = "Updated Name"
            };

            BatchType? capturedBatchType = null;

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdateBatchTypeAsync(It.IsAny<BatchType>()))
                .Callback<BatchType>(bt => capturedBatchType = bt)
                .ReturnsAsync((BatchType bt) => bt);
            _mapperMock.Setup(x => x.Map<BatchTypeDto>(It.IsAny<BatchType>()))
                .Returns(new BatchTypeDto());

            var beforeUpdate = DateTime.UtcNow;

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedBatchType.ShouldNotBeNull();
            capturedBatchType.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
        }
    }
}
