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
    public class CreatePhaseTypeHandlerTests
    {
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreatePhaseTypeHandler _handler;

        public CreatePhaseTypeHandlerTests()
        {
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreatePhaseTypeHandler(_batchRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesPhaseType()
        {
            // Arrange
            var command = new CreatePhaseTypeCommand
            {
                Name = "Foundation Phase"
            };

            var phaseType = new PhaseType
            {
                Id = 1,
                Name = command.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var phaseTypeDto = new PhaseTypeDto
            {
                Id = 1,
                Name = command.Name
            };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, null))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.AddPhaseTypeAsync(It.IsAny<PhaseType>()))
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
            result.Data.Name.ShouldBe(command.Name);
            _batchRepositoryMock.Verify(x => x.AddPhaseTypeAsync(It.IsAny<PhaseType>()), Times.Once);
        }

        [Theory]
        [InlineData("Foundation")]
        [InlineData("Advanced")]
        [InlineData("Specialization")]
        [InlineData("Project Phase")]
        public async Task Handle_DifferentPhaseNames_CreatesSuccessfully(string phaseName)
        {
            // Arrange
            var command = new CreatePhaseTypeCommand { Name = phaseName };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(phaseName, null))
                .ReturnsAsync(false);
            _batchRepositoryMock.Setup(x => x.AddPhaseTypeAsync(It.IsAny<PhaseType>()))
                .ReturnsAsync((PhaseType pt) => { pt.Id = 1; return pt; });
            _mapperMock.Setup(x => x.Map<PhaseTypeDto>(It.IsAny<PhaseType>()))
                .Returns((PhaseType pt) => new PhaseTypeDto { Name = pt.Name });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Data.Name.ShouldBe(phaseName);
        }

        [Fact]
        public async Task Handle_DuplicateName_ReturnsFailure()
        {
            // Arrange
            var command = new CreatePhaseTypeCommand { Name = "Existing Phase" };

            _batchRepositoryMock.Setup(x => x.PhaseTypeNameExistsAsync(command.Name, null))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("already exists");
            _batchRepositoryMock.Verify(x => x.AddPhaseTypeAsync(It.IsAny<PhaseType>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyName_ReturnsFailure()
        {
            // Arrange
            var command = new CreatePhaseTypeCommand { Name = "" };

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
            var command = new CreatePhaseTypeCommand { Name = null };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
        }
    }
}
