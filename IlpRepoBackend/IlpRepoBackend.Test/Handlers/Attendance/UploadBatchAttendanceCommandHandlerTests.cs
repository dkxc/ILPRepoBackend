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
    public class UploadBatchAttendanceCommandHandlerTests
    {
        private readonly Mock<IAttendanceRepository> _attendanceRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly UploadBatchAttendanceCommandHandler _handler;

        public UploadBatchAttendanceCommandHandlerTests()
        {
            _attendanceRepositoryMock = new Mock<IAttendanceRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _handler = new UploadBatchAttendanceCommandHandler(
                _attendanceRepositoryMock.Object,
                _traineeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUpload_ProcessesAllRecords()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.A,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "jane@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.A
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 },
                new Trainee { Id = 2, Email = "jane@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldContain("Successfully processed 2 attendance records");
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list => list.Count == 2)), Times.Once);
        }

        [Fact]
        public async Task Handle_NoTraineesInBatch_ReturnsFailure()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto { TraineeEmail = "john@example.com", Date = "2024-01-01" }
                }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1))
                .ReturnsAsync(new List<Trainee>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("No trainees found");
        }

        [Fact]
        public async Task Handle_InvalidTraineeEmail_ReturnsPartialFailure()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "invalid@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Processed 1 records");
            result.Message.ShouldContain("invalid@example.com");
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list => list.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task Handle_CaseInsensitiveEmailMatch_ProcessesRecords()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "JOHN@EXAMPLE.COM",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list => list.Count == 1 && list[0].TraineeId == 1)), Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleRecordsForSameTrainee_ProcessesAll()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-02",
                        Forenoon = AttendanceStatus.A,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-03",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.A
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list => list.Count == 3)), Times.Once);
        }

        [Fact]
        public async Task Handle_MixedAttendanceStatuses_ProcessesCorrectly()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.A
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-02",
                        Forenoon = AttendanceStatus.NA,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.Any(a => a.Date == new DateOnly(2024, 1, 1) &&
                           a.ForenoonStatus == AttendanceStatus.P &&
                           a.AfternoonStatus == AttendanceStatus.A) &&
                    list.Any(a => a.Date == new DateOnly(2024, 1, 2) &&
                           a.ForenoonStatus == AttendanceStatus.NA &&
                           a.AfternoonStatus == AttendanceStatus.P))), Times.Once);
        }

        [Fact]
        public async Task Handle_AllInvalidEmails_ReturnsFailure()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "invalid1@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "invalid2@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Processed 0 records");
            result.Message.ShouldContain("invalid1@example.com");
            result.Message.ShouldContain("invalid2@example.com");
        }

        [Fact]
        public async Task Handle_EmptyUploadData_NoUpsert()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>()
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldContain("Successfully processed 0 attendance records");
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(It.IsAny<List<Domain.Entities.Attendance>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateEmailsInUpload_ListsOnce()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "invalid@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "invalid@example.com",
                        Date = "2024-01-02",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            // Count occurrences of the invalid email in the message
            var messageCount = result.Message.Split(new[] { "invalid@example.com" }, StringSplitOptions.None).Length - 1;
            messageCount.ShouldBe(1); // Should appear only once due to Distinct()
        }

        [Fact]
        public async Task Handle_MultipleTraineesWithDifferentDates_ProcessesCorrectly()
        {
            // Arrange
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = 1,
                UploadData = new List<UploadAttendanceDto>
                {
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "john@example.com",
                        Date = "2024-01-01",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "jane@example.com",
                        Date = "2024-01-02",
                        Forenoon = AttendanceStatus.A,
                        Afternoon = AttendanceStatus.P
                    },
                    new UploadAttendanceDto
                    {
                        TraineeEmail = "bob@example.com",
                        Date = "2024-01-03",
                        Forenoon = AttendanceStatus.P,
                        Afternoon = AttendanceStatus.A
                    }
                }
            };

            var trainees = new List<Trainee>
            {
                new Trainee { Id = 1, Email = "john@example.com", BatchId = 1 },
                new Trainee { Id = 2, Email = "jane@example.com", BatchId = 1 },
                new Trainee { Id = 3, Email = "bob@example.com", BatchId = 1 }
            };

            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(trainees);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldContain("Successfully processed 3 attendance records");
            _attendanceRepositoryMock.Verify(x => x.UpsertAttendanceRange(
                It.Is<List<Domain.Entities.Attendance>>(list =>
                    list.Count == 3 &&
                    list.Any(a => a.TraineeId == 1) &&
                    list.Any(a => a.TraineeId == 2) &&
                    list.Any(a => a.TraineeId == 3))), Times.Once);
        }
    }
}
