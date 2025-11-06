using AutoMapper;
using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Users;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Users
{
    public class DeleteUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly DeleteUserCommandHandler _handler;

        public DeleteUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new DeleteUserCommandHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_DeletesUser()
        {
            // Arrange
            var userId = 1;
            var command = new DeleteUserCommand { Id = userId };

            _userRepositoryMock.Setup(x => x.DeleteAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();
            _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidUserId_ReturnsFalse()
        {
            // Arrange
            var userId = 999;
            var command = new DeleteUserCommand { Id = userId };

            _userRepositoryMock.Setup(x => x.DeleteAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
            _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public async Task Handle_VariousUserIds_CallsRepositoryWithCorrectId(int userId)
        {
            // Arrange
            var command = new DeleteUserCommand { Id = userId };

            _userRepositoryMock.Setup(x => x.DeleteAsync(userId))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }
    }
}
