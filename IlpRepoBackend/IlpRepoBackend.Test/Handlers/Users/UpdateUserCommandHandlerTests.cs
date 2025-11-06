using AutoMapper;
using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.CustomeException;
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
    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesUser()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Username = "updateduser",
                Role = UserRole.Admin,
                IsActive = true,
                Password = null
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "olduser",
                Email = "test@example.com",
                PasswordHash = "oldHash",
                Role = UserRole.Trainee,
                IsActive = false
            };

            var updatedUser = new User
            {
                Id = 1,
                Username = command.Username,
                Email = "test@example.com",
                Role = command.Role,
                IsActive = command.IsActive
            };

            var userDto = new UserDto
            {
                Id = 1,
                Username = command.Username,
                Role = command.Role,
                IsActive = command.IsActive
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(command.Username))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(updatedUser);
            _mapperMock.Setup(x => x.Map<UserDto>(updatedUser))
                .Returns(userDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Username.ShouldBe(command.Username);
            result.Role.ShouldBe(command.Role);
            result.IsActive.ShouldBe(command.IsActive);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 999,
                Username = "testuser",
                Role = UserRole.Admin,
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UsernameAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Username = "existinguser",
                Role = UserRole.Admin,
                IsActive = true
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "olduser",
                Email = "test@example.com"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(command.Username))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("existinguser");
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPassword_UpdatesPasswordHash()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Username = "testuser",
                Password = "newpassword",
                Role = UserRole.Admin,
                IsActive = true
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "oldHash"
            };

            User? capturedUser = null;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(command.Username))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync((User u) => u);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(new UserDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.PasswordHash.ShouldNotBe("oldHash");
            capturedUser.PasswordHash.ShouldNotBe(command.Password);
        }

        [Fact]
        public async Task Handle_WithoutPassword_KeepsExistingPasswordHash()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Username = "testuser",
                Password = null,
                Role = UserRole.Admin,
                IsActive = true
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "existingHash"
            };

            User? capturedUser = null;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync((User u) => u);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(new UserDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.PasswordHash.ShouldBe("existingHash");
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.TeamLead)]
        [InlineData(UserRole.Trainee)]
        public async Task Handle_DifferentRoles_UpdatesCorrectly(UserRole role)
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Username = "testuser",
                Role = role,
                IsActive = true
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "testuser",
                Role = UserRole.Trainee
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(existingUser);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(new UserDto { Role = role });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Role.ShouldBe(role);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_DifferentIsActiveValues_UpdatesCorrectly(bool isActive)
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = 1,
                Username = "testuser",
                Role = UserRole.Admin,
                IsActive = isActive
            };

            var existingUser = new User
            {
                Id = 1,
                Username = "testuser",
                IsActive = !isActive
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(command.Id))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(existingUser);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(new UserDto { IsActive = isActive });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsActive.ShouldBe(isActive);
        }
    }
}
