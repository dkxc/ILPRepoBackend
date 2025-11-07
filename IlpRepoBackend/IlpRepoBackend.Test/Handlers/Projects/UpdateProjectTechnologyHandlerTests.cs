using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Handler.Projects;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Projects
{
    public class UpdateProjectTechnologyHandlerTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly UpdateProjectTechnologyHandler _handler;

        public UpdateProjectTechnologyHandlerTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _handler = new UpdateProjectTechnologyHandler(_projectRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesTechnology()
        {
            // Arrange
            var command = new UpdateProjectTechnologyCommand(1, "React, Node.js, MongoDB");
            var project = new Project { Id = 1, ProjectName = "Test", Technology = "Old Tech" };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Project>())).ReturnsAsync(project);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldBeTrue();
            result.Message.ShouldContain("updated successfully");
            _projectRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Project>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProjectTechnologyCommand(999, ".NET");

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Project?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("not found");
            _projectRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Project>()), Times.Never);
        }

        [Theory]
        [InlineData(".NET Core, React, SQL Server")]
        [InlineData("Java, Spring Boot, PostgreSQL")]
        [InlineData("Python, Django, MongoDB")]
        public async Task Handle_DifferentTechnologies_UpdatesCorrectly(string technology)
        {
            // Arrange
            var command = new UpdateProjectTechnologyCommand(1, technology);
            var project = new Project { Id = 1, Technology = "Old Tech" };

            Project? capturedProject = null;

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => capturedProject = p)
                .ReturnsAsync((Project p) => p);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedProject.ShouldNotBeNull();
            capturedProject.Technology.ShouldBe(technology);
        }

        [Fact]
        public async Task Handle_UpdatesTimestamp()
        {
            // Arrange
            var command = new UpdateProjectTechnologyCommand(1, "New Tech");
            var project = new Project
            {
                Id = 1,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var oldTimestamp = project.UpdatedAt;
            Project? capturedProject = null;

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => capturedProject = p)
                .ReturnsAsync((Project p) => p);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedProject.ShouldNotBeNull();
            capturedProject.UpdatedAt.ShouldBeGreaterThan(oldTimestamp);
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProjectTechnologyCommand(1, "New Tech");
            var project = new Project { Id = 1 };

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Error updating technology stack");
        }
    }
}
