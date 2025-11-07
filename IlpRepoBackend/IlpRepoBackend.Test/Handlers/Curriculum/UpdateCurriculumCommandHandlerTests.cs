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
    public class UpdateCurriculumCommandHandlerTests
    {
        private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateCurriculumCommandHandler _handler;

        public UpdateCurriculumCommandHandlerTests()
        {
            _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateCurriculumCommandHandler(
                _curriculumRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesCurriculum()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };

            var existingCurriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "Old Title",
                Description = "Old Description",
                Instructor = "Old Instructor",
                Color = "#000000",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var updateDto = new UpdateCurriculumDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Instructor = "Updated Instructor",
                Color = "#FFFFFF",
                Start = DateTime.UtcNow.AddDays(1),
                End = DateTime.UtcNow.AddDays(1).AddHours(2)
            };

            var updatedCurriculumDto = new CurriculumDto
            {
                Id = curriculumId,
                Title = updateDto.Title,
                Description = updateDto.Description,
                Instructor = updateDto.Instructor,
                Color = updateDto.Color,
                Start = updateDto.Start,
                End = updateDto.End
            };

            var command = new UpdateCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId,
                UpdateCurriculumDto = updateDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(existingCurriculum);
            _mapperMock.Setup(x => x.Map(updateDto, existingCurriculum))
                .Returns(existingCurriculum);
            _curriculumRepositoryMock.Setup(x => x.UpdateAsync(existingCurriculum))
                .ReturnsAsync(existingCurriculum);
            _mapperMock.Setup(x => x.Map<CurriculumDto>(existingCurriculum))
                .Returns(updatedCurriculumDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Title.ShouldBe(updateDto.Title);
            result.Data.Description.ShouldBe(updateDto.Description);
            result.Data.Instructor.ShouldBe(updateDto.Instructor);
            
            _curriculumRepositoryMock.Verify(x => x.UpdateAsync(existingCurriculum), Times.Once);
        }

        [Fact]
        public async Task Handle_BatchNotFound_ReturnsFailure()
        {
            // Arrange
            var batchId = 999;
            var curriculumId = 10;

            var updateDto = new UpdateCurriculumDto
            {
                Title = "Updated Title",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new UpdateCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId,
                UpdateCurriculumDto = updateDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync((Batch)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Batch not found");
            
            _curriculumRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Curriculum>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CurriculumNotFound_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 999;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };

            var updateDto = new UpdateCurriculumDto
            {
                Title = "Updated Title",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new UpdateCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId,
                UpdateCurriculumDto = updateDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync((Domain.Entities.Curriculum)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Curriculum event not found in this batch");
            
            _curriculumRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Curriculum>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CurriculumBelongsToDifferentBatch_ReturnsFailure()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };

            var existingCurriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = 2, // Different batch
                Title = "Test",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var updateDto = new UpdateCurriculumDto
            {
                Title = "Updated Title",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new UpdateCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId,
                UpdateCurriculumDto = updateDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(existingCurriculum);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Curriculum event not found in this batch");
            
            _curriculumRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Curriculum>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdatesAllFields()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };

            var existingCurriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "Old",
                Description = "Old",
                Instructor = "Old",
                Color = "#000000",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var updateDto = new UpdateCurriculumDto
            {
                Title = "New Title",
                Description = "New Description",
                Instructor = "New Instructor",
                Color = "#FF0000",
                Start = DateTime.UtcNow.AddDays(2),
                End = DateTime.UtcNow.AddDays(2).AddHours(3)
            };

            var command = new UpdateCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId,
                UpdateCurriculumDto = updateDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(existingCurriculum);
            _mapperMock.Setup(x => x.Map(updateDto, existingCurriculum))
                .Callback<UpdateCurriculumDto, Domain.Entities.Curriculum>((dto, entity) =>
                {
                    entity.Title = dto.Title;
                    entity.Description = dto.Description;
                    entity.Instructor = dto.Instructor;
                    entity.Color = dto.Color;
                    entity.Start = dto.Start;
                    entity.End = dto.End;
                })
                .Returns(existingCurriculum);
            _curriculumRepositoryMock.Setup(x => x.UpdateAsync(existingCurriculum))
                .ReturnsAsync(existingCurriculum);
            _mapperMock.Setup(x => x.Map<CurriculumDto>(existingCurriculum))
                .Returns(new CurriculumDto
                {
                    Id = curriculumId,
                    Title = updateDto.Title,
                    Description = updateDto.Description,
                    Instructor = updateDto.Instructor,
                    Color = updateDto.Color,
                    Start = updateDto.Start,
                    End = updateDto.End
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Title.ShouldBe(updateDto.Title);
            result.Data.Description.ShouldBe(updateDto.Description);
            result.Data.Instructor.ShouldBe(updateDto.Instructor);
            result.Data.Color.ShouldBe(updateDto.Color);
            result.Data.Start.ShouldBe(updateDto.Start);
            result.Data.End.ShouldBe(updateDto.End);
        }

        [Fact]
        public async Task Handle_RepositoryException_ThrowsException()
        {
            // Arrange
            var batchId = 1;
            var curriculumId = 10;
            var batch = new Batch { Id = batchId, BatchName = "Test Batch" };

            var existingCurriculum = new Domain.Entities.Curriculum
            {
                Id = curriculumId,
                BatchId = batchId,
                Title = "Test",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var updateDto = new UpdateCurriculumDto
            {
                Title = "Updated",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };

            var command = new UpdateCurriculumCommand
            {
                Id = curriculumId,
                BatchId = batchId,
                UpdateCurriculumDto = updateDto
            };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(batchId))
                .ReturnsAsync(batch);
            _curriculumRepositoryMock.Setup(x => x.GetByIdAsync(curriculumId))
                .ReturnsAsync(existingCurriculum);
            _mapperMock.Setup(x => x.Map(updateDto, existingCurriculum))
                .Returns(existingCurriculum);
            _curriculumRepositoryMock.Setup(x => x.UpdateAsync(existingCurriculum))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}
