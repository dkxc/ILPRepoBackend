using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Handler.Projects;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Projects
{
    public class DeleteProjectHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IMentorForPRojectRepository> _mentorForProjectRepositoryMock;
        private readonly Mock<IPocForAProjectRepository> _pocForProjectRepositoryMock;
        private readonly Mock<IProjecTeamRepository> _projectTeamRepositoryMock;
        private readonly DeleteProjectHandler _handler;

        public DeleteProjectHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _mentorForProjectRepositoryMock = new Mock<IMentorForPRojectRepository>();
            _pocForProjectRepositoryMock = new Mock<IPocForAProjectRepository>();
            _projectTeamRepositoryMock = new Mock<IProjecTeamRepository>();

            _handler = new DeleteProjectHandler(
                _projectRepositoryMock.Object,
                _mentorForProjectRepositoryMock.Object,
                _pocForProjectRepositoryMock.Object,
                _projectTeamRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidProjectId_DeletesProject()
        {
            // Arrange
            var command = new DeleteProjectCommand(1);
            var project = new Project { Id = 1, ProjectName = "Test Project" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectTeamRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1)).Returns(Task.CompletedTask);
            _mentorForProjectRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1)).Returns(Task.CompletedTask);
            _pocForProjectRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1)).Returns(Task.CompletedTask);
            _projectRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeTrue();
            result.Message.ShouldContain("deleted successfully");
            _projectRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteProjectCommand(999);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Project?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _projectRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DeletesRelatedData_InCorrectOrder()
        {
            // Arrange
            var command = new DeleteProjectCommand(1);
            var project = new Project { Id = 1 };

            var deletionOrder = new List<string>();

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectTeamRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1))
                .Callback(() => deletionOrder.Add("ProjectTeam"))
                .Returns(Task.CompletedTask);
            _mentorForProjectRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1))
                .Callback(() => deletionOrder.Add("Mentors"))
                .Returns(Task.CompletedTask);
            _pocForProjectRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1))
                .Callback(() => deletionOrder.Add("POCs"))
                .Returns(Task.CompletedTask);
            _projectRepositoryMock.Setup(x => x.DeleteAsync(1))
                .Callback(() => deletionOrder.Add("Project"))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            deletionOrder.Count.ShouldBe(4);
            deletionOrder[3].ShouldBe("Project"); // Project should be deleted last
        }

        [Fact]
        public async Task Handle_ExceptionDuringDeletion_ReturnsFailure()
        {
            // Arrange
            var command = new DeleteProjectCommand(1);
            var project = new Project { Id = 1 };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectTeamRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Failed to delete project");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task Handle_DifferentProjectIds_DeletesCorrectly(int projectId)
        {
            // Arrange
            var command = new DeleteProjectCommand(projectId);
            var project = new Project { Id = projectId };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _projectTeamRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(projectId)).Returns(Task.CompletedTask);
            _mentorForProjectRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(projectId)).Returns(Task.CompletedTask);
            _pocForProjectRepositoryMock.Setup(x => x.DeleteAllByProjectIdAsync(projectId)).Returns(Task.CompletedTask);
            _projectRepositoryMock.Setup(x => x.DeleteAsync(projectId)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _projectRepositoryMock.Verify(x => x.DeleteAsync(projectId), Times.Once);
        }
    }
}
