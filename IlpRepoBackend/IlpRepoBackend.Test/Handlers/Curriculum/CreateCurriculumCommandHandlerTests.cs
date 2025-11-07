using AutoMapper;
using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Curriculum
{
    public class CreateCurriculumCommandHandlerTests
    {
        private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateCurriculumCommandHandler _handler;

        public CreateCurriculumCommandHandlerTests()
        {
            _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateCurriculumCommandHandler(
                _curriculumRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesCurriculum()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            
            var createDto = new CreateCurriculumDto
            {
                Title = "Introduction to C#",
                Description = "Basic C# programming concepts",
                Instructor = "John Doe",
                Color = "#FF5733",
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = 1,
                BatchId = batchId,
                Title = createDto.Title,
                Description = createDto.Description,
                Instructor = createDto.Instructor,
                Color = createDto.Color,
                Start = createDto.Start,
                End = createDto.End
            };

            var curriculumDto = new CurriculumDto
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                Instructor = createDto.Instructor,
                Color = createDto.Color,
                Start = createDto.Start,
                End = createDto.End
            };

            var command = new CreateCurriculumCommand
            {
                BatchId = batchId,
                CreateCurriculumDto = createDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<Domain.Entities.Curriculum>(createDto))
                .Returns(curriculum);
            _curriculumRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Curriculum>()))
                .ReturnsAsync(curriculum);
            _mapperMock.Setup(x => x.Map<CurriculumDto>(curriculum))
                .Returns(curriculumDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Title.ShouldBe(createDto.Title);
            result.Data.Description.ShouldBe(createDto.Description);
            result.Data.Instructor.ShouldBe(createDto.Instructor);
            result.Data.Color.ShouldBe(createDto.Color);
            
            _curriculumRepositoryMock.Verify(x => x.AddAsync(It.Is<Domain.Entities.Curriculum>(
                c => c.BatchId == batchId && c.Title == createDto.Title)), Times.Once);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var batchId = 999;
            var createDto = new CreateCurriculumDto
            {
                Title = "Introduction to C#",
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            var command = new CreateCurriculumCommand
            {
                BatchId = batchId,
                CreateCurriculumDto = createDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync((Batch)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Batch not found");
            
            _curriculumRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Curriculum>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SetsBatchIdCorrectly()
        {
            // Arrange
            var batchId = 5;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            
            var createDto = new CreateCurriculumDto
            {
                Title = "Advanced Topics",
                Start = DateTime.UtcNow.AddDays(2),
                End = DateTime.UtcNow.AddDays(2).AddHours(3)
            };

            var curriculum = new Domain.Entities.Curriculum
            {
                Title = createDto.Title,
                Start = createDto.Start,
                End = createDto.End
            };

            var command = new CreateCurriculumCommand
            {
                BatchId = batchId,
                CreateCurriculumDto = createDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<Domain.Entities.Curriculum>(createDto))
                .Returns(curriculum);
            _curriculumRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Curriculum>()))
                .ReturnsAsync(curriculum);
            _mapperMock.Setup(x => x.Map<CurriculumDto>(It.IsAny<Domain.Entities.Curriculum>()))
                .Returns(new CurriculumDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _curriculumRepositoryMock.Verify(x => x.AddAsync(It.Is<Domain.Entities.Curriculum>(
                c => c.BatchId == batchId)), Times.Once);
        }

        [Fact]
        public async Task Handle_WithAllOptionalFields_CreatesCurriculum()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            
            var createDto = new CreateCurriculumDto
            {
                Title = "Complete Course",
                Description = "Full detailed description",
                Instructor = "Jane Smith",
                Color = "#00FF00",
                Start = DateTime.UtcNow.AddDays(5),
                End = DateTime.UtcNow.AddDays(5).AddHours(4)
            };

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = 1,
                BatchId = batchId,
                Title = createDto.Title,
                Description = createDto.Description,
                Instructor = createDto.Instructor,
                Color = createDto.Color,
                Start = createDto.Start,
                End = createDto.End
            };

            var curriculumDto = new CurriculumDto
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                Instructor = createDto.Instructor,
                Color = createDto.Color,
                Start = createDto.Start,
                End = createDto.End
            };

            var command = new CreateCurriculumCommand
            {
                BatchId = batchId,
                CreateCurriculumDto = createDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<Domain.Entities.Curriculum>(createDto))
                .Returns(curriculum);
            _curriculumRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Curriculum>()))
                .ReturnsAsync(curriculum);
            _mapperMock.Setup(x => x.Map<CurriculumDto>(curriculum))
                .Returns(curriculumDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Description.ShouldBe(createDto.Description);
            result.Data.Instructor.ShouldBe(createDto.Instructor);
            result.Data.Color.ShouldBe(createDto.Color);
        }

        [Fact]
        public async Task Handle_WithMinimalFields_CreatesCurriculum()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            
            var createDto = new CreateCurriculumDto
            {
                Title = "Quick Session",
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(1).AddHours(1)
            };

            var curriculum = new Domain.Entities.Curriculum
            {
                Id = 1,
                BatchId = batchId,
                Title = createDto.Title,
                Start = createDto.Start,
                End = createDto.End
            };

            var curriculumDto = new CurriculumDto
            {
                Id = 1,
                Title = createDto.Title,
                Start = createDto.Start,
                End = createDto.End
            };

            var command = new CreateCurriculumCommand
            {
                BatchId = batchId,
                CreateCurriculumDto = createDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<Domain.Entities.Curriculum>(createDto))
                .Returns(curriculum);
            _curriculumRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Curriculum>()))
                .ReturnsAsync(curriculum);
            _mapperMock.Setup(x => x.Map<CurriculumDto>(curriculum))
                .Returns(curriculumDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Title.ShouldBe(createDto.Title);
        }

        [Fact]
        public async Task Handle_RepositoryException_ThrowsException()
        {
            // Arrange
            var batchId = 1;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };
            
            var createDto = new CreateCurriculumDto
            {
                Title = "Test",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new CreateCurriculumCommand
            {
                BatchId = batchId,
                CreateCurriculumDto = createDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _mapperMock.Setup(x => x.Map<Domain.Entities.Curriculum>(createDto))
                .Returns(new Domain.Entities.Curriculum());
            _curriculumRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Curriculum>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}
