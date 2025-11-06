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
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _authServiceMock = new Mock<IAuthService>();
            _handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _mapperMock.Object, _authServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesUser()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                Role = UserRole.TeamLead,
                IsActive = true
            };

            var hashedPassword = "hashedPassword123";
            var createdUser = new User
            {
                Id = 1,
                Username = command.Username,
                Email = command.Email,
                PasswordHash = hashedPassword,
                Role = command.Role,
                IsActive = command.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userDto = new UserDto
            {
                Id = 1,
                Username = command.Username,
                Email = command.Email,
                Role = command.Role,
                IsActive = command.IsActive
            };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(command.Username))
                .ReturnsAsync(false);
            _authServiceMock.Setup(x => x.HashPassword(command.Password))
                .Returns(hashedPassword);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);
            _mapperMock.Setup(x => x.Map<UserDto>(createdUser))
                .Returns(userDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(1);
            result.Username.ShouldBe(command.Username);
            result.Email.ShouldBe(command.Email);
            result.Role.ShouldBe(command.Role);
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Username = "testuser",
                Email = "existing@example.com",
                Password = "password123",
                Role = UserRole.TeamLead
            };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("Email");
            exception.Message.ShouldContain(command.Email);
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UsernameExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Username = "existinguser",
                Email = "test@example.com",
                Password = "password123",
                Role = UserRole.TeamLead
            };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(command.Email))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(command.Username))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );

            exception.Message.ShouldContain("Username");
            exception.Message.ShouldContain(command.Username);
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.TeamLead)]
        [InlineData(UserRole.Trainee)]
        public async Task Handle_DifferentRoles_CreatesUserWithCorrectRole(UserRole role)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                Role = role,
                IsActive = true
            };

            var createdUser = new User
            {
                Id = 1,
                Username = command.Username,
                Email = command.Email,
                Role = role
            };

            var userDto = new UserDto
            {
                Id = 1,
                Role = role
            };

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _authServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashedPassword");
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Role.ShouldBe(role);
        }

        [Fact]
        public async Task Handle_PasswordIsHashed_BeforeStoringUser()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "plainPassword",
                Role = UserRole.TeamLead
            };

            var hashedPassword = "hashedPassword123";
            User? capturedUser = null;

            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.UsernameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _authServiceMock.Setup(x => x.HashPassword(command.Password))
                .Returns(hashedPassword);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync((User u) => u);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(new UserDto());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.PasswordHash.ShouldBe(hashedPassword);
            capturedUser.PasswordHash.ShouldNotBe(command.Password);
            _authServiceMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        }
    }
}
