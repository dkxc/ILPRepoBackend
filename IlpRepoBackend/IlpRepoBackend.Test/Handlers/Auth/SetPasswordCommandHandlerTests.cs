using IlpRepoBackend.Application.Command.Auth;
using IlpRepoBackend.Application.Handler.Auth;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Auth
{
    public class SetPasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly SetPasswordCommandHandler _handler;

        public SetPasswordCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthService>();
            _handler = new SetPasswordCommandHandler(
                _userRepositoryMock.Object,
                _authServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidToken_SetsPassword()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var token = "valid-token";
            var newPassword = "NewSecurePassword123!";
            var hashedPassword = "hashed-password";

            var user = new User
            {
                Id = userId,
                Email = email,
                Username = "testuser"
            };

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.HashPassword(newPassword))
                .Returns(hashedPassword);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var command = new SetPasswordCommand(email, token, newPassword);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldBe("Password set successfully.");

            _authServiceMock.Verify(x => x.HashPassword(newPassword), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidToken_ReturnsFailure()
        {
            // Arrange
            var email = "test@example.com";
            var token = "invalid-token";
            var newPassword = "password";
            int userId = 0;

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(false);

            var command = new SetPasswordCommand(email, token, newPassword);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Invalid or expired token.");

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var token = "valid-token";
            var newPassword = "password";

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            var command = new SetPasswordCommand(email, token, newPassword);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("User not found or email mismatch.");

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmailMismatch_ReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var requestEmail = "request@example.com";
            var userEmail = "different@example.com";
            var token = "valid-token";
            var newPassword = "password";

            var user = new User
            {
                Id = userId,
                Email = userEmail,
                Username = "testuser"
            };

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var command = new SetPasswordCommand(requestEmail, token, newPassword);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("User not found or email mismatch.");

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdatesTimestamp()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var token = "valid-token";
            var newPassword = "password";

            var user = new User
            {
                Id = userId,
                Email = email,
                Username = "testuser",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var oldTimestamp = user.UpdatedAt;

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.HashPassword(newPassword))
                .Returns("hashed");

            User capturedUser = null;
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync((User u) => u);

            var command = new SetPasswordCommand(email, token, newPassword);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.UpdatedAt.ShouldBeGreaterThan(oldTimestamp);
        }

        [Fact]
        public async Task Handle_HashesPassword()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var token = "valid-token";
            var newPassword = "PlainTextPassword123!";
            var hashedPassword = "$2a$11$hashedversion";

            var user = new User { Id = userId, Email = email };

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.HashPassword(newPassword))
                .Returns(hashedPassword);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var command = new SetPasswordCommand(email, token, newPassword);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _authServiceMock.Verify(x => x.HashPassword(newPassword), Times.Once);
        }

        [Fact]
        public async Task Handle_StoresHashedPassword()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var token = "valid-token";
            var newPassword = "password";
            var hashedPassword = "hashed-password-value";

            var user = new User { Id = userId, Email = email };

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.HashPassword(newPassword))
                .Returns(hashedPassword);

            User capturedUser = null;
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .ReturnsAsync((User u) => u);

            var command = new SetPasswordCommand(email, token, newPassword);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.ShouldNotBeNull();
            capturedUser.PasswordHash.ShouldBe(hashedPassword);
        }

        [Theory]
        [InlineData("Password123!")]
        [InlineData("ComplexP@ssw0rd")]
        [InlineData("MySecurePass!2023")]
        public async Task Handle_DifferentPasswords_HashesEach(string password)
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var token = "valid-token";
            var hashedPassword = $"hashed-{password}";

            var user = new User { Id = userId, Email = email };

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(true);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.HashPassword(password))
                .Returns(hashedPassword);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var command = new SetPasswordCommand(email, token, password);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _authServiceMock.Verify(x => x.HashPassword(password), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidatesTokenBeforeAnyOperation()
        {
            // Arrange
            var email = "test@example.com";
            var token = "token";
            var password = "password";
            int userId = 0;

            _authServiceMock.Setup(x => x.ValidatePasswordSetupToken(token, out userId))
                .Returns(false);

            var command = new SetPasswordCommand(email, token, password);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _authServiceMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        }
    }
}
