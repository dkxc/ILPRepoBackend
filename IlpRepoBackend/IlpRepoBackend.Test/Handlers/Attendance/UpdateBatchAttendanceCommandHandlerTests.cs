using IlpRepoBackend.Application.Command.Attendance;
using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Handler.Attendance;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Attendance
{
    public class UpdateBatchAttendanceCommandHandlerTests
    {
        private readonly Mock<IAttendanceRepository> _attendanceRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly UpdateBatchAttendanceCommandHandler _handler;

        public UpdateBatchAttendanceCommandHandlerTests()
        {
            _attendanceRepositoryMock = new Mock<IAttendanceRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _handler = new UpdateBatchAttendanceCommandHandler(
                _attendanceRepositoryMock.Object,
                _traineeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesAttendance()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1, 2 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-02",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.A,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBe(4); // 2 trainees x 2 days = 4 records
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list => list.Count == 4)), Times.Once);
        }

        [Fact]
        public async Task Handle_NoTraineeIds_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int>(),
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto()
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("At least one trainee must be selected");
        }

        [Fact]
        public async Task Handle_NullTraineeIds_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = null,
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto()
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("At least one trainee must be selected");
        }

        [Fact]
        public async Task Handle_UpdatesExistingRecords()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.A,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            var existingRecord = new Domain.Entities.Attendance
            {
                Id = 10,
                TraineeId = 1,
                Date = new DateOnly(2024, 1, 1),
                ForenoonStatus = AttendanceStatus.P,
                AfternoonStatus = AttendanceStatus.P
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance> { existingRecord });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.Any(a => a.Id == 10 &&
                           a.ForenoonStatus == AttendanceStatus.A &&
                           a.AfternoonStatus == AttendanceStatus.P))), Times.Once);
        }

        [Fact]
        public async Task Handle_CreatesNewRecordsWhenNotExist()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.A
                    }
                }
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.Any(a => a.Id == 0 && a.TraineeId == 1))), Times.Once);
        }

        [Fact]
        public async Task Handle_OnlyForenoonProvided_UpdatesOnlyForenoon()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.A,
                        Afternoon = null
                    }
                }
            };

            var existingRecord = new Domain.Entities.Attendance
            {
                Id = 10,
                TraineeId = 1,
                Date = new DateOnly(2024, 1, 1),
                ForenoonStatus = AttendanceStatus.P,
                AfternoonStatus = AttendanceStatus.P
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance> { existingRecord });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.Any(a => a.ForenoonStatus == AttendanceStatus.A &&
                           a.AfternoonStatus == AttendanceStatus.P))), Times.Once);
        }

        [Fact]
        public async Task Handle_OnlyAfternoonProvided_UpdatesOnlyAfternoon()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = null,
                        Afternoon = AttendanceStatus.A
                    }
                }
            };

            var existingRecord = new Domain.Entities.Attendance
            {
                Id = 10,
                TraineeId = 1,
                Date = new DateOnly(2024, 1, 1),
                ForenoonStatus = AttendanceStatus.P,
                AfternoonStatus = AttendanceStatus.P
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance> { existingRecord });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.Any(a => a.ForenoonStatus == AttendanceStatus.P &&
                           a.AfternoonStatus == AttendanceStatus.A))), Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleTraineesMultipleDays_CreatesAllRecords()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1, 2, 3 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-05",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.A,
                        Afternoon = AttendanceStatus.A
                    }
                }
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBe(15); // 3 trainees x 5 days = 15 records
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list => list.Count == 15)), Times.Once);
        }

        [Fact]
        public async Task Handle_NAStatus_SetsCorrectStatus()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1 },
                    StartDate = "2024-01-01",
                    EndDate = "2024-01-01",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.NA,
                        Afternoon = AttendanceStatus.NA
                    }
                }
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.All(a => a.ForenoonStatus == AttendanceStatus.NA &&
                           a.AfternoonStatus == AttendanceStatus.NA))), Times.Once);
        }

        [Fact]
        public async Task Handle_SingleDay_UpdatesOneDay()
        {
            // Arrange
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = 1,
                UpdateData = new UpdateBatchAttendanceDto
                {
                    TraineeIds = new List<int> { 1 },
                    StartDate = "2024-01-15",
                    EndDate = "2024-01-15",
                    Status = new UpdateStatusDto
                    {
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            _attendanceRepositoryMock.Setup(x => x.GetByTraineeIdsAndDateRange(
                It.IsAny<List<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Domain.Entities.Attendance>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBe(1);
        }
    }
}
