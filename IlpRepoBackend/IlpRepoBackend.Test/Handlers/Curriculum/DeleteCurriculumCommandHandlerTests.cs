using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Handler;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Curriculum
{
    public class DeleteCurriculumCommandHandlerTests
    {
        private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
        private readonly DeleteCurriculumCommandHandler _handler;

        public DeleteCurriculumCommandHandlerTests()
        {
            _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
            _handler = new DeleteCurriculumCommandHandler(_curriculumRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_DeletesCurriculum()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "Test Curriculum",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(2)
            };

            var command = new DeleteCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId
            };

            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(curriculum);
            _curriculumRepositoryMock.Setup(x => x.DeleteAsync(curriculumId))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeTrue();
            result.Message.ShouldContain("deleted successfully");
            
            _curriculumRepositoryMock.Verify(x => x.DeleteAsync(curriculumId), Times.Once);
        }

        [Fact]
        public async Task Handle_CurriculumNotFound_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 999;

            var command = new DeleteCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId
            };

            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync((Domain.Entities.Curriculum)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Curriculum event not found");
            
            _curriculumRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CurriculumBelongsToDifferentBatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = 2, // Different batch
                Title = "Test Curriculum",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(2)
            };

            var command = new DeleteCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId
            };

            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(curriculum);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Curriculum event not found in this batch");
            
            _curriculumRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidatesOwnership()
        {
            // Arrange
            var batchId = 5;
            var curriculumId = 10;

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "Test",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new DeleteCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId
            };

            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(curriculum);
            _curriculumRepositoryMock.Setup(x => x.DeleteAsync(curriculumId))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _curriculumRepositoryMock.Verify(x => x.GetByIdAsync(curriculumId), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryException_ThrowsException()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "Test",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new DeleteCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId
            };

            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(curriculum);
            _curriculumRepositoryMock.Setup(x => x.DeleteAsync(curriculumId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeletesOnlySpecifiedCurriculum()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "To Delete",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new DeleteCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId
            };

            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(curriculum);
            _curriculumRepositoryMock.Setup(x => x.DeleteAsync(curriculumId))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _curriculumRepositoryMock.Verify(x => x.DeleteAsync(curriculumId), Times.Once);
            _curriculumRepositoryMock.Verify(x => x.DeleteAsync(It.Is<int>(id => id != curriculumId)), Times.Never);
        }
    }
}
