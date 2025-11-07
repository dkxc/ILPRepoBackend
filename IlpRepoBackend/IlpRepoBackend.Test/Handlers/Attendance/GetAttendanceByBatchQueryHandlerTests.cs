using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Handler.Attendance;
using IlpRepoBackend.Application.Query.Attendance;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Attendance
{
    public class GetAttendanceByBatchQueryHandlerTests
    {
        private readonly Mock<IAttendanceRepository> _attendanceRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly GetAttendanceByBatchQueryHandler _handler;

        public GetAttendanceByBatchQueryHandlerTests()
        {
            _attendanceRepositoryMock = new Mock<IAttendanceRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _handler = new GetAttendanceByBatchQueryHandler(
                _attendanceRepositoryMock.Object,
                _traineeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidBatchWithTrainees_ReturnsAttendance()
        {
            // Arrange
            var batchId = 1;
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 3);
            var query = new GetAttendanceByBatchQuery
            {
                BatchId = batchId,
                StartDate = startDate,
                EndDate = endDate
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, User = new User { Username = "John Doe" } },
                new Trainee { Id = 2, BatchId = batchId, User = new User { Username = "Jane Smith" } }
            };

            var attendanceRecords = new List<Domain.Entities.Attendance>
            {
                new Domain.Entities.Attendance
                {
                    TraineeId = 1,
                    Date = new DateOnly(2024, 1, 1),
                    ForenoonStatus = AttendanceStatus.A,
                    AfternoonStatus = AttendanceStatus.P
                },
                new Domain.Entities.Attendance
                {
                    TraineeId = 2,
                    Date = new DateOnly(2024, 1, 2),
                    ForenoonStatus = AttendanceStatus.P,
                    AfternoonStatus = AttendanceStatus.A
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), startDate, endDate)).ReturnsAsync(attendanceRecords);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(2);
            result.Data[0].TraineeName.ShouldBe("John Doe");
            result.Data[0].Dates.Count.ShouldBe(3); // 3 days
            result.Data[0].Dates["2024-01-01"].Forenoon.ShouldBe(AttendanceStatus.A);
            result.Data[0].Dates["2024-01-01"].Afternoon.ShouldBe(AttendanceStatus.P);
        }

        [Fact]
        public async Task Handle_NoTraineesInBatch_ReturnsEmptyList()
        {
            // Arrange
            var batchId = 1;
            var query = new GetAttendanceByBatchQuery { BatchId = batchId };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId))
                .ReturnsAsync(new List<Trainee>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(0);
            result.Message.ShouldContain("No trainees found");
        }

        [Fact]
        public async Task Handle_NoDateRangeProvided_UsesLast7Days()
        {
            // Arrange
            var batchId = 1;
            var query = new GetAttendanceByBatchQuery { BatchId = batchId };
            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, User = new User { Username = "John" } }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Dates.Count.ShouldBe(7); // Last 7 days
        }

        [Fact]
        public async Task Handle_NoAttendanceRecords_DefaultsToPresent()
        {
            // Arrange
            var batchId = 1;
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);
            var query = new GetAttendanceByBatchQuery
            {
                BatchId = batchId,
                StartDate = startDate,
                EndDate = endDate
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, User = new User { Username = "John" } }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), startDate, endDate))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Dates["2024-01-01"].Forenoon.ShouldBe(AttendanceStatus.P);
            result.Data[0].Dates["2024-01-01"].Afternoon.ShouldBe(AttendanceStatus.P);
        }

        [Fact]
        public async Task Handle_MultipleTraineesAndDates_ReturnsCompleteData()
        {
            // Arrange
            var batchId = 1;
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 2);
            var query = new GetAttendanceByBatchQuery
            {
                BatchId = batchId,
                StartDate = startDate,
                EndDate = endDate
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, User = new User { Username = "John" } },
                new Trainee { Id = 2, BatchId = batchId, User = new User { Username = "Jane" } },
                new Trainee { Id = 3, BatchId = batchId, User = new User { Username = "Bob" } }
            };

            var attendanceRecords = new List<Domain.Entities.Attendance>
            {
                new Domain.Entities.Attendance
                {
                    TraineeId = 1,
                    Date = new DateOnly(2024, 1, 1),
                    ForenoonStatus = AttendanceStatus.A,
                    AfternoonStatus = AttendanceStatus.P
                },
                new Domain.Entities.Attendance
                {
                    TraineeId = 2,
                    Date = new DateOnly(2024, 1, 1),
                    ForenoonStatus = AttendanceStatus.P,
                    AfternoonStatus = AttendanceStatus.A
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), startDate, endDate)).ReturnsAsync(attendanceRecords);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Count.ShouldBe(3);
            result.Data[0].Dates.Count.ShouldBe(2);
            result.Data[1].Dates.Count.ShouldBe(2);
            result.Data[2].Dates.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Handle_MixedAttendanceStatuses_ReturnsCorrectStatuses()
        {
            // Arrange
            var batchId = 1;
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);
            var query = new GetAttendanceByBatchQuery
            {
                BatchId = batchId,
                StartDate = startDate,
                EndDate = endDate
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, User = new User { Username = "John" } }
            };

            var attendanceRecords = new List<Domain.Entities.Attendance>
            {
                new Domain.Entities.Attendance
                {
                    TraineeId = 1,
                    Date = new DateOnly(2024, 1, 1),
                    ForenoonStatus = AttendanceStatus.NA,
                    AfternoonStatus = AttendanceStatus.A
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), startDate, endDate)).ReturnsAsync(attendanceRecords);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Dates["2024-01-01"].Forenoon.ShouldBe(AttendanceStatus.NA);
            result.Data[0].Dates["2024-01-01"].Afternoon.ShouldBe(AttendanceStatus.A);
        }

        [Fact]
        public async Task Handle_SingleDayQuery_ReturnsOneDayData()
        {
            // Arrange
            var batchId = 1;
            var date = new DateOnly(2024, 1, 15);
            var query = new GetAttendanceByBatchQuery
            {
                BatchId = batchId,
                StartDate = date,
                EndDate = date
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, BatchId = batchId, User = new User { Username = "John" } }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), date, date))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data[0].Dates.Count.ShouldBe(1);
            result.Data[0].Dates.ContainsKey("2024-01-15").ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_PassesCorrectTraineeIds()
        {
            // Arrange
            var batchId = 1;
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);
            var query = new GetAttendanceByBatchQuery
            {
                BatchId = batchId,
                StartDate = startDate,
                EndDate = endDate
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 5, BatchId = batchId, User = new User { Username = "John" } },
                new Trainee { Id = 10, BatchId = batchId, User = new User { Username = "Jane" } }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(batchId)).ReturnsAsync(trainees);
            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), startDate, endDate))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.GetByTraineeIdsAndDateRange(
                It.Is<List<int>>(ids => ids.Contains(5) && ids.Contains(10)), startDate, endDate), Times.Once);
        }
    }
}
