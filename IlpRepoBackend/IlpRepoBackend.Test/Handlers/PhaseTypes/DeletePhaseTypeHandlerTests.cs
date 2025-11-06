using IlpRepoBackend.Application.Command.PhaseTypes;
using IlpRepoBackend.Application.Handler.PhaseTypes;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.PhaseTypes
{
    public class DeletePhaseTypeHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly DeletePhaseTypeHandler _handler;

        public DeletePhaseTypeHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _handler = new DeletePhaseTypeHandler(_batchRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidPhaseTypeId_DeletesSuccessfully()
        {
            // Arrange
            var phaseTypeId = 1;

            _batchRepositoryMock.Setup(x => x.DeletePhaseTypeAsync(phaseTypeId))
                .ReturnsAsync(true);

            var command = new DeletePhaseTypeCommand { Id = phaseTypeId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();
            _batchRepositoryMock.Verify(x => x.DeletePhaseTypeAsync(phaseTypeId), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidPhaseTypeId_ReturnsFalse()
        {
            // Arrange
            var phaseTypeId = 999;
            
            _batchRepositoryMock.Setup(x => x.DeletePhaseTypeAsync(phaseTypeId))
                .ReturnsAsync(false);

            var command = new DeletePhaseTypeCommand { Id = phaseTypeId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
            _batchRepositoryMock.Verify(x => x.DeletePhaseTypeAsync(phaseTypeId), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Handle_DifferentPhaseTypeIds_CallsRepositoryCorrectly(int phaseTypeId)
        {
            // Arrange
            _batchRepositoryMock.Setup(x => x.DeletePhaseTypeAsync(phaseTypeId))
                .ReturnsAsync(true);

            var command = new DeletePhaseTypeCommand { Id = phaseTypeId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _batchRepositoryMock.Verify(x => x.DeletePhaseTypeAsync(phaseTypeId), Times.Once);
        }

        [Fact]
        public async Task Handle_ZeroId_ReturnsFalse()
        {
            // Arrange
            var command = new DeletePhaseTypeCommand { Id = 0 };

            _batchRepositoryMock.Setup(x => x.DeletePhaseTypeAsync(0))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_NegativeId_ReturnsFalse()
        {
            // Arrange
            var command = new DeletePhaseTypeCommand { Id = -1 };

            _batchRepositoryMock.Setup(x => x.DeletePhaseTypeAsync(-1))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
        }
    }
}
