using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Curriculum
{
    public class GetCurriculumByBatchIdQueryHandlerTests
    {
        private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetCurriculumByBatchIdQueryHandler _handler;

        public GetCurriculumByBatchIdQueryHandlerTests()
        {
            _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetCurriculumByBatchIdQueryHandler(
                _curriculumRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_CurriculumExists_ReturnsAllCurriculumForBatch()
        {
            // Arrange
            var batchId = 1;

            var curriculumList = new List<Domain.Entities.Curriculum>
            {
                new Domain.Entities.Curriculum
                {
                    Id = 1,
                    BatchId = batchId,
                    Title = "Introduction to C#",
                    Description = "Basics of C#",
                    Instructor = "John Doe",
                    Color = "#FF5733",
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(2)
                },
                new Domain.Entities.Curriculum
                {
                    Id = 2,
                    BatchId = batchId,
                    Title = "Advanced C#",
                    Description = "Advanced concepts",
                    Instructor = "Jane Smith",
                    Color = "#33FF57",
                    Start = DateTime.UtcNow.AddDays(1),
                    End = DateTime.UtcNow.AddDays(1).AddHours(3)
                },
                new Domain.Entities.Curriculum
                {
                    Id = 3,
                    BatchId = batchId,
                    Title = "ASP.NET Core",
                    Description = "Web development",
                    Instructor = "Bob Johnson",
                    Color = "#3357FF",
                    Start = DateTime.UtcNow.AddDays(2),
                    End = DateTime.UtcNow.AddDays(2).AddHours(4)
                }
            };

            var curriculumDtoList = new List<CurriculumDto>
            {
                new CurriculumDto
                {
                    Id = 1,
                    Title = "Introduction to C#",
                    Description = "Basics of C#",
                    Instructor = "John Doe",
                    Color = "#FF5733",
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(2)
                },
                new CurriculumDto
                {
                    Id = 2,
                    Title = "Advanced C#",
                    Description = "Advanced concepts",
                    Instructor = "Jane Smith",
                    Color = "#33FF57",
                    Start = DateTime.UtcNow.AddDays(1),
                    End = DateTime.UtcNow.AddDays(1).AddHours(3)
                },
                new CurriculumDto
                {
                    Id = 3,
                    Title = "ASP.NET Core",
                    Description = "Web development",
                    Instructor = "Bob Johnson",
                    Color = "#3357FF",
                    Start = DateTime.UtcNow.AddDays(2),
                    End = DateTime.UtcNow.AddDays(2).AddHours(4)
                }
            };

            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(curriculumList);
            _mapperMock.Setup(x => x.Map<List<CurriculumDto>>(curriculumList))
                .Returns(curriculumDtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(3);
            result.Data[0].Title.ShouldBe("Introduction to C#");
            result.Data[1].Title.ShouldBe("Advanced C#");
            result.Data[2].Title.ShouldBe("ASP.NET Core");
        }

        [Fact]
        public async Task Handle_NoCurriculumExists_ReturnsEmptyList()
        {
            // Arrange
            var batchId = 1;
            var emptyCurriculumList = new List<Domain.Entities.Curriculum>();
            var emptyDtoList = new List<CurriculumDto>();

            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(emptyCurriculumList);
            _mapperMock.Setup(x => x.Map<List<CurriculumDto>>(emptyCurriculumList))
                .Returns(emptyDtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_MapsAllFieldsCorrectly()
        {
            // Arrange
            var batchId = 1;
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow.AddHours(2);

            var curriculumList = new List<Domain.Entities.Curriculum>
            {
                new Domain.Entities.Curriculum
                {
                    Id = 1,
                    BatchId = batchId,
                    Title = "Complete Course",
                    Description = "Full description",
                    Instructor = "Expert Instructor",
                    Color = "#FF00FF",
                    Start = startTime,
                    End = endTime
                }
            };

            var curriculumDtoList = new List<CurriculumDto>
            {
                new CurriculumDto
                {
                    Id = 1,
                    Title = "Complete Course",
                    Description = "Full description",
                    Instructor = "Expert Instructor",
                    Color = "#FF00FF",
                    Start = startTime,
                    End = endTime
                }
            };

            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(curriculumList);
            _mapperMock.Setup(x => x.Map<List<CurriculumDto>>(curriculumList))
                .Returns(curriculumDtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].Id.ShouldBe(1);
            result.Data[0].Title.ShouldBe("Complete Course");
            result.Data[0].Description.ShouldBe("Full description");
            result.Data[0].Instructor.ShouldBe("Expert Instructor");
            result.Data[0].Color.ShouldBe("#FF00FF");
            result.Data[0].Start.ShouldBe(startTime);
            result.Data[0].End.ShouldBe(endTime);
        }

        [Fact]
        public async Task Handle_MultipleSessionsOnSameDay_ReturnsAll()
        {
            // Arrange
            var batchId = 1;
            var today = DateTime.UtcNow.Date;

            var curriculumList = new List<Domain.Entities.Curriculum>
            {
                new Domain.Entities.Curriculum
                {
                    Id = 1,
                    BatchId = batchId,
                    Title = "Morning Session",
                    Start = today.AddHours(9),
                    End = today.AddHours(12)
                },
                new Domain.Entities.Curriculum
                {
                    Id = 2,
                    BatchId = batchId,
                    Title = "Afternoon Session",
                    Start = today.AddHours(13),
                    End = today.AddHours(16)
                },
                new Domain.Entities.Curriculum
                {
                    Id = 3,
                    BatchId = batchId,
                    Title = "Evening Session",
                    Start = today.AddHours(17),
                    End = today.AddHours(20)
                }
            };

            var curriculumDtoList = curriculumList.Select(c => new CurriculumDto
            {
                Id = c.Id,
                Title = c.Title,
                Start = c.Start,
                End = c.End
            }).ToList();

            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(curriculumList);
            _mapperMock.Setup(x => x.Map<List<CurriculumDto>>(curriculumList))
                .Returns(curriculumDtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            result.Data[0].Title.ShouldBe("Morning Session");
            result.Data[1].Title.ShouldBe("Afternoon Session");
            result.Data[2].Title.ShouldBe("Evening Session");
        }

        [Fact]
        public async Task Handle_WithOptionalFields_HandlesNullValues()
        {
            // Arrange
            var batchId = 1;

            var curriculumList = new List<Domain.Entities.Curriculum>
            {
                new Domain.Entities.Curriculum
                {
                    Id = 1,
                    BatchId = batchId,
                    Title = "Minimal Session",
                    Description = null,
                    Instructor = null,
                    Color = null,
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(1)
                }
            };

            var curriculumDtoList = new List<CurriculumDto>
            {
                new CurriculumDto
                {
                    Id = 1,
                    Title = "Minimal Session",
                    Description = null,
                    Instructor = null,
                    Color = null,
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(1)
                }
            };

            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(curriculumList);
            _mapperMock.Setup(x => x.Map<List<CurriculumDto>>(curriculumList))
                .Returns(curriculumDtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            result.Data[0].Title.ShouldBe("Minimal Session");
        }

        [Fact]
        public async Task Handle_DifferentBatchIds_ReturnsOnlyMatchingBatch()
        {
            // Arrange
            var batchId = 1;

            var curriculumList = new List<Domain.Entities.Curriculum>
            {
                new Domain.Entities.Curriculum
                {
                    Id = 1,
                    BatchId = batchId,
                    Title = "Batch 1 Session",
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(1)
                }
            };

            var curriculumDtoList = new List<CurriculumDto>
            {
                new CurriculumDto
                {
                    Id = 1,
                    Title = "Batch 1 Session",
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(1)
                }
            };

            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(curriculumList);
            _mapperMock.Setup(x => x.Map<List<CurriculumDto>>(curriculumList))
                .Returns(curriculumDtoList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(1);
            _curriculumRepositoryMock.Verify(x => x.GetByBatchIdAsync(batchId), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryException_ThrowsException()
        {
            // Arrange
            var batchId = 1;
            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };

            _curriculumRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
                await _handler.Handle(query, CancellationToken.None));
        }
    }
}
