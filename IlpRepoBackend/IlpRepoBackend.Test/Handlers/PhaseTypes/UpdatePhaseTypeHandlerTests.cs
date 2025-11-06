using AutoMapper;
using IlpRepoBackend.Application.Command.PhaseTypes;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.PhaseTypes;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.PhaseTypes
{
    public class UpdatePhaseTypeHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdatePhaseTypeHandler _handler;

        public UpdatePhaseTypeHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdatePhaseTypeHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesPhaseType()
        {
            // Arrange
            var command = new UpdatePhaseTypeCommand
            {
                Id = 1,
                Name = "Updated Foundation Phase"
            };

            var phaseType = new PhaseType
            {
                Id = 1,
                Name = command.Name,
                UpdatedAt = DateTime.UtcNow
            };

            var phaseTypeDto = new PhaseTypeDto
            {
                Id = 1,
                Name = command.Name
            };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(phaseType))
                .Returns(phaseTypeDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(1);
            result.Data.Name.ShouldBe("Updated Foundation Phase");
            result.Message.ShouldContain("successfully");
            _batchRepositoryMock.Verify(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ReturnsFailure()
        {
            // Arrange
            var command = new UpdatePhaseTypeCommand
            {
                Id = 0,
                Name = "Test Phase"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid id");
            _batchRepositoryMock.Verify(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NegativeId_ReturnsFailure()
        {
            // Arrange
            var command = new UpdatePhaseTypeCommand
            {
                Id = -1,
                Name = "Test Phase"
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
            var command = new UpdatePhaseTypeCommand
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
            var command = new UpdatePhaseTypeCommand
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
            var command = new UpdatePhaseTypeCommand
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
            var command = new UpdatePhaseTypeCommand
            {
                Id = 1,
                Name = "Existing Phase"
            };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
            _batchRepositoryMock.Verify(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()), Times.Never);
        }

        [Fact]
        public async Task Handle_PhaseTypeNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdatePhaseTypeCommand
            {
                Id = 999,
                Name = "Test Phase"
            };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()))
                .ReturnsAsync((PhaseType?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
        }

        [Theory]
        [InlineData("Foundation Phase")]
        [InlineData("Advanced Phase")]
        [InlineData("Specialization Phase")]
        public async Task Handle_DifferentNames_UpdatesCorrectly(string name)
        {
            // Arrange
            var command = new UpdatePhaseTypeCommand
            {
                Id = 1,
                Name = name
            };

            var phaseType = new PhaseType { Id = 1, Name = name };
            var phaseTypeDto = new PhaseTypeDto { Id = 1, Name = name };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(name, 1))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()))
                .ReturnsAsync(phaseType);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(It.IsAny<PhaseType>()))
                .Returns(phaseTypeDto);

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
            var command = new UpdatePhaseTypeCommand
            {
                Id = 1,
                Name = "Test Phase"
            };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()))
                .ReturnsAsync(new PhaseType { Id = 1, Name = command.Name });
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(It.IsAny<PhaseType>()))
                .Returns(new PhaseTypeDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(
                x => x.PhaseTypeNameExistsAsync(command.Name, command.Id),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_UpdatesTimestamp()
        {
            // Arrange
            var command = new UpdatePhaseTypeCommand
            {
                Id = 1,
                Name = "Updated Name"
            };

            PhaseType? capturedPhaseType = null;

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, command.Id))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.UpdatePhaseTypeAsync(It.IsAny<PhaseType>()))
                .Callback<PhaseType>(pt => capturedPhaseType = pt)
                .ReturnsAsync((PhaseType pt) => pt);
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(It.IsAny<PhaseType>()))
                .Returns(new PhaseTypeDto());

            var beforeUpdate = DateTime.UtcNow;

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedPhaseType.ShouldNotBeNull();
            capturedPhaseType.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
        }
    }
}
