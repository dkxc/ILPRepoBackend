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
    public class CreateBatchTypeHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateBatchTypeHandler _handler;

        public CreateBatchTypeHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateBatchTypeHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesBatchType()
        {
            // Arrange
            var command = new CreateBatchTypeCommand
            {
                Name = "Full Stack Development"
            };

            var batchType = new BatchType
            {
                Id = 1,
                Name = command.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var batchTypeDto = new BatchTypeDto
            {
                Id = 1,
                Name = command.Name
            };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, null))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.AddBatchTypeAsync(It.IsAny<BatchType>()))
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
            result.Data.Name.ShouldBe(command.Name);
            _batchRepositoryMock.Verify(x => x.AddBatchTypeAsync(It.IsAny<BatchType>()), Times.Once);
        }

        [Theory]
        [InlineData("Full Stack Development")]
        [InlineData("Data Science")]
        [InlineData("DevOps Engineering")]
        [InlineData("Cloud Architecture")]
        [InlineData("Cyber Security")]
        public async Task Handle_DifferentBatchTypeNames_CreatesSuccessfully(string name)
        {
            // Arrange
            var command = new CreateBatchTypeCommand { Name = name };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(name, null))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.AddBatchTypeAsync(It.IsAny<BatchType>()))
                .ReturnsAsync((BatchType bt) => { bt.Id = 1; return bt; });
            _mapperMock.Setup(x => x.Map<BatchTypeDto>(It.IsAny<BatchType>()))
                .Returns((BatchType bt) => new BatchTypeDto { Name = bt.Name });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Data.Name.ShouldBe(name);
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBatchTypeCommand { Name = "Existing Batch Type" };

            _batchRepositoryMock.Setup(x => x.BatchTypeNameExistsAsync(command.Name, null))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
            _batchRepositoryMock.Verify(x => x.AddBatchTypeAsync(It.IsAny<BatchType>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBatchTypeCommand { Name = "" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("required");
        }

        [Fact]
        public async Task Handle_NullName_ReturnsFailure()
        {
            // Arrange
            var command = new CreateBatchTypeCommand { Name = null };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
        }
    }
}
