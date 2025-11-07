using IlpRepoBackend.Application.Dto.AdminDashboard;
using IlpRepoBackend.Application.Handler.AdminDashboard;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.AdminDashboard
{
    public class GetTrainingHoursReportQueryHandlerTests
    {
        private readonly Mock<ITrainingScheduleRepository> _trainingScheduleRepositoryMock;
        private readonly Mock<IBatchTypeRepository> _batchTypeRepositoryMock;
        private readonly GetTrainingHoursReportQueryHandler _handler;

        public GetTrainingHoursReportQueryHandlerTests()
        {
            _trainingScheduleRepositoryMock = new Mock<ITrainingScheduleRepository>();
            _batchTypeRepositoryMock = new Mock<IBatchTypeRepository>();
            _handler = new GetTrainingHoursReportQueryHandler(
                _trainingScheduleRepositoryMock.Object,
                _batchTypeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidBatchTypeId_ReturnsSummaryForSpecificType()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            var batchType = new BatchType { Id = batchTypeId, Name = "Full Stack Development" };

            var schedules = new List<TrainingSchedule>
            {
                new TrainingSchedule
                {
                    Id = 1,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 1, 15),
                    Hours = 8,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "ILP Batch 1",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                },
                new TrainingSchedule
                {
                    Id = 2,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 2, 20),
                    Hours = 6,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "ILP Batch 1",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                }
            };

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(schedules);
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(batchType);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.BatchTypeName.ShouldBe("Full Stack Development");
            result.TotalHours.ShouldBe(14);
            result.BatchDetails.Count.ShouldBe(1);
            result.BatchDetails[0].BatchName.ShouldBe("ILP Batch 1");
            result.BatchDetails[0].TotalTrainingHours.ShouldBe(14);
        }

        [Fact]
        public async Task Handle_WithNullBatchTypeId_ReturnsAllBatchTypes()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            var schedules = new List<TrainingSchedule>
            {
                new TrainingSchedule
                {
                    Id = 1,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 1, 15),
                    Hours = 8,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "ILP Batch 1",
                        BatchTypeId = 1,
                        BatchType = new BatchType { Id = 1, Name = "Full Stack" }
                    }
                },
                new TrainingSchedule
                {
                    Id = 2,
                    BatchId = 2,
                    TrainingDate = new DateTime(2024, 2, 20),
                    Hours = 6,
                    Batch = new Batch
                    {
                        Id = 2,
                        BatchName = "Data Science Batch",
                        BatchTypeId = 2,
                        BatchType = new BatchType { Id = 2, Name = "Data Science" }
                    }
                }
            };

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(null, startDate, endDate))
                .ReturnsAsync(schedules);

            var query = new GetTrainingHoursReportQuery(null, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.BatchTypeName.ShouldBe("All Batch Types");
            result.TotalHours.ShouldBe(14);
            result.BatchDetails.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_NoSchedulesFound_ReturnsZeroHours()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            var batchType = new BatchType { Id = batchTypeId, Name = "Full Stack" };

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(new List<TrainingSchedule>());
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(batchType);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalHours.ShouldBe(0);
            result.BatchDetails.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_MultipleBatchesSameType_AggregatesCorrectly()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            var batchType = new BatchType { Id = batchTypeId, Name = "Full Stack" };

            var schedules = new List<TrainingSchedule>
            {
                new TrainingSchedule
                {
                    Id = 1,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 1, 15),
                    Hours = 8,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "ILP Batch 1",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                },
                new TrainingSchedule
                {
                    Id = 2,
                    BatchId = 2,
                    TrainingDate = new DateTime(2024, 2, 20),
                    Hours = 6,
                    Batch = new Batch
                    {
                        Id = 2,
                        BatchName = "ILP Batch 2",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                },
                new TrainingSchedule
                {
                    Id = 3,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 3, 10),
                    Hours = 7,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "ILP Batch 1",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                }
            };

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(schedules);
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(batchType);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalHours.ShouldBe(21);
            result.BatchDetails.Count.ShouldBe(2);
            
            var batch1Details = result.BatchDetails.FirstOrDefault(b => b.BatchName == "ILP Batch 1");
            batch1Details.ShouldNotBeNull();
            batch1Details.TotalTrainingHours.ShouldBe(15); // 8 + 7

            var batch2Details = result.BatchDetails.FirstOrDefault(b => b.BatchName == "ILP Batch 2");
            batch2Details.ShouldNotBeNull();
            batch2Details.TotalTrainingHours.ShouldBe(6);
        }

        [Fact]
        public async Task Handle_BatchTypeNotFound_ReturnsUnknownBatchType()
        {
            // Arrange
            var batchTypeId = 999;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(new List<TrainingSchedule>());
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync((BatchType?)null);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.BatchTypeName.ShouldBe("Unknown");
        }

        [Fact]
        public async Task Handle_DateRangeSpanningMultipleMonths_CalculatesCorrectly()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            var batchType = new BatchType { Id = batchTypeId, Name = "Full Stack" };

            var schedules = Enumerable.Range(1, 12).Select(month => new TrainingSchedule
            {
                Id = month,
                BatchId = 1,
                TrainingDate = new DateTime(2024, month, 15),
                Hours = 8,
                Batch = new Batch
                {
                    Id = 1,
                    BatchName = "Year-Long Batch",
                    BatchTypeId = batchTypeId,
                    BatchType = batchType
                }
            }).ToList();

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(schedules);
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(batchType);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalHours.ShouldBe(96); // 12 months * 8 hours
            result.BatchDetails[0].TotalTrainingHours.ShouldBe(96);
        }

        [Fact]
        public async Task Handle_LargeNumberOfSchedules_HandlesEfficiently()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            var batchType = new BatchType { Id = batchTypeId, Name = "Full Stack" };

            var schedules = Enumerable.Range(1, 100).Select(i => new TrainingSchedule
            {
                Id = i,
                BatchId = (i % 5) + 1, // 5 different batches
                TrainingDate = startDate.AddDays(i),
                Hours = 8,
                Batch = new Batch
                {
                    Id = (i % 5) + 1,
                    BatchName = $"Batch {(i % 5) + 1}",
                    BatchTypeId = batchTypeId,
                    BatchType = batchType
                }
            }).ToList();

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(schedules);
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(batchType);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalHours.ShouldBe(800); // 100 schedules * 8 hours
            result.BatchDetails.Count.ShouldBe(5);
        }

        [Fact]
        public async Task Handle_VariousHourAmounts_SumsCorrectly()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            var batchType = new BatchType { Id = batchTypeId, Name = "Full Stack" };

            var schedules = new List<TrainingSchedule>
            {
                new TrainingSchedule
                {
                    Id = 1,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 1, 15),
                    Hours = 4,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "Test Batch",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                },
                new TrainingSchedule
                {
                    Id = 2,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 2, 20),
                    Hours = 6,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "Test Batch",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                },
                new TrainingSchedule
                {
                    Id = 3,
                    BatchId = 1,
                    TrainingDate = new DateTime(2024, 3, 10),
                    Hours = 8,
                    Batch = new Batch
                    {
                        Id = 1,
                        BatchName = "Test Batch",
                        BatchTypeId = batchTypeId,
                        BatchType = batchType
                    }
                }
            };

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(schedules);
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(batchType);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalHours.ShouldBe(18); // 4 + 6 + 8
        }

        [Fact]
        public async Task Handle_CallsRepositoriesCorrectly()
        {
            // Arrange
            var batchTypeId = 1;
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate))
                .ReturnsAsync(new List<TrainingSchedule>());
            _batchTypeRepositoryMock
                .Setup(x => x.GetByIdAsync(batchTypeId))
                .ReturnsAsync(new BatchType { Id = batchTypeId, Name = "Test" });

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _trainingScheduleRepositoryMock.Verify(
                x => x.GetByBatchTypeAndDateRangeAsync(batchTypeId, startDate, endDate),
                Times.Once);
            _batchTypeRepositoryMock.Verify(x => x.GetByIdAsync(batchTypeId), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullBatchType_DoesNotCallBatchTypeRepository()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

            _trainingScheduleRepositoryMock
                .Setup(x => x.GetByBatchTypeAndDateRangeAsync(null, startDate, endDate))
                .ReturnsAsync(new List<TrainingSchedule>());

            var query = new GetTrainingHoursReportQuery(null, startDate, endDate);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _batchTypeRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
