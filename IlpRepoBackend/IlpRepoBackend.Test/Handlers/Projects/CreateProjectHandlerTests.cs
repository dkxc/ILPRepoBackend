using AutoMapper;
using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Handler.Projects;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Projects
{
    public class CreateProjectHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IBatchRepository> _batchRepositoryMock;
        private readonly Mock<ITraineeRepository> _traineeRepositoryMock;
        private readonly Mock<IMentorRepository> _mentorRepositoryMock;
        private readonly Mock<IMentorForPRojectRepository> _mentorForProjectRepositoryMock;
        private readonly Mock<IPocForAProjectRepository> _pocForProjectRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IPocRepository> _pocRepositoryMock;
        private readonly Mock<IProjecTeamRepository> _projectTeamRepositoryMock;
        private readonly CreateProjectHandler _handler;

        public CreateProjectHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _batchRepositoryMock = new Mock<IBatchRepository>();
            _traineeRepositoryMock = new Mock<ITraineeRepository>();
            _mentorRepositoryMock = new Mock<IMentorRepository>();
            _mentorForProjectRepositoryMock = new Mock<IMentorForPRojectRepository>();
            _pocForProjectRepositoryMock = new Mock<IPocForAProjectRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _pocRepositoryMock = new Mock<IPocRepository>();
            _projectTeamRepositoryMock = new Mock<IProjecTeamRepository>();

            _handler = new CreateProjectHandler(
                _projectRepositoryMock.Object,
                _batchRepositoryMock.Object,
                _traineeRepositoryMock.Object,
                _mentorRepositoryMock.Object,
                _pocForProjectRepositoryMock.Object,
                _pocRepositoryMock.Object,
                _userRepositoryMock.Object,
                _mapperMock.Object,
                _mentorForProjectRepositoryMock.Object,
                _projectTeamRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesProject()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                Technology = ".NET",
                Status = ProjectStatus.Live,
                Progress = 50,
                BatchId = 1,
                TeamMembers = new List<string> { "trainee1" },
                TeamLeadName = "lead1",
                ScrumMasterName = "scrum1",
                Mentors = new List<Mentor>(),
                Pocs = new List<Poc>()
            };
            var command = new CreateProjectCommand(dto);

            var batch = new Batch { Id = 1, BatchName = "Batch 1" };
            var trainee = new Trainee { Id = 1, BatchId = 1, UserId = 1 };
            var leadUser = new User { Id = 2, Username = "lead1" };
            var scrumUser = new User { Id = 3, Username = "scrum1" };
            var teamLead = new Trainee { Id = 2, UserId = 2, BatchId = 1 };
            var scrumMaster = new Trainee { Id = 3, UserId = 3, BatchId = 1 };
            var project = new Project { Id = 1, ProjectName = dto.ProjectName };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1))
                .ReturnsAsync(new List<Trainee> { trainee, teamLead, scrumMaster });
            _traineeRepositoryMock.Setup(x => x.GetByName("trainee1")).ReturnsAsync(trainee);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("lead1")).ReturnsAsync(leadUser);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("scrum1")).ReturnsAsync(scrumUser);
            _projectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1)).ReturnsAsync(project);
            _projectTeamRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectTeam>())).ReturnsAsync(new ProjectTeam());
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>())).Returns(new ProjectDto { Id = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            _projectRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidBatchId_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                BatchId = 999
            };
            var command = new CreateProjectCommand(dto);

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Batch?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Batch does not exist");
            _projectRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TeamLeadNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                BatchId = 1,
                TeamLeadName = "nonexistent",
                TeamMembers = new List<string>()
            };
            var command = new CreateProjectCommand(dto);

            var batch = new Batch { Id = 1 };
            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(new List<Trainee>());
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync("nonexistent")).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Team Lead");
            result.Message.ShouldContain("not found");
        }

        [Fact]
        public async Task Handle_TeamMemberNotInBatch_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                BatchId = 1,
                TeamMembers = new List<string> { "trainee1" }
            };
            var command = new CreateProjectCommand(dto);

            var batch = new Batch { Id = 1 };
            var trainee = new Trainee { Id = 1, BatchId = 2 }; // Different batch

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(new List<Trainee>());
            _traineeRepositoryMock.Setup(x => x.GetByName("trainee1")).ReturnsAsync(trainee);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("does not belong to batch");
        }

        [Fact]
        public async Task Handle_CreatesNewMentorIfNotExists()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                BatchId = 1,
                TeamMembers = new List<string>(),
                Mentors = new List<Mentor>
                {
                    new Mentor { Name = "New Mentor", Email = "mentor@test.com" }
                }
            };
            var command = new CreateProjectCommand(dto);

            var batch = new Batch { Id = 1 };
            var project = new Project { Id = 1 };
            var newMentor = new Mentor { Id = 1, Name = "New Mentor" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(new List<Trainee>());
            _mentorRepositoryMock.Setup(x => x.GetByEmailAsync("mentor@test.com")).ReturnsAsync((Mentor?)null);
            _mentorRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Mentor>())).ReturnsAsync(newMentor);
            _mentorRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(newMentor);
            _projectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1)).ReturnsAsync(project);
            _mentorForProjectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<MenterForAProject>()))
                .ReturnsAsync(new MenterForAProject());
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>())).Returns(new ProjectDto());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _mentorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Mentor>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreatesNewPocIfNotExists()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                BatchId = 1,
                TeamMembers = new List<string>(),
                Pocs = new List<Poc>
                {
                    new Poc { Name = "New POC", Email = "poc@test.com" }
                }
            };
            var command = new CreateProjectCommand(dto);

            var batch = new Batch { Id = 1 };
            var project = new Project { Id = 1 };
            var newPoc = new Poc { Id = 1, Name = "New POC" };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(new List<Trainee>());
            _pocRepositoryMock.Setup(x => x.GetByEmailAsync("poc@test.com")).ReturnsAsync((Poc?)null);
            _pocRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Poc>())).ReturnsAsync(newPoc);
            _projectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1)).ReturnsAsync(project);
            _pocForProjectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PocsForProject>()))
                .ReturnsAsync(new PocsForProject());
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>())).Returns(new ProjectDto());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _pocRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Poc>()), Times.Once);
        }

        [Theory]
        [InlineData(ProjectStatus.NotLive)]
        [InlineData(ProjectStatus.Live)]
        [InlineData(ProjectStatus.Completed)]
        public async Task Handle_DifferentStatuses_CreatesCorrectly(ProjectStatus status)
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                ProjectName = "Test Project",
                BatchId = 1,
                Status = status,
                TeamMembers = new List<string>()
            };
            var command = new CreateProjectCommand(dto);

            var batch = new Batch { Id = 1 };
            var project = new Project { Id = 1, Status = status };

            _batchRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(batch);
            _traineeRepositoryMock.Setup(x => x.GetByBatchIdAsync(1)).ReturnsAsync(new List<Trainee>());
            _projectRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.GetProjectWithDetailsAsync(1)).ReturnsAsync(project);
            _mapperMock.Setup(x => x.Map<ProjectDto>(It.IsAny<Project>()))
                .Returns(new ProjectDto { Status = status });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.Data.Status.ShouldBe(status);
        }
    }
}
