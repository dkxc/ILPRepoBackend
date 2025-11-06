using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Auth;
using IlpRepoBackend.Application.Query.Auth;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Auth
{
    public class LoginUserQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly LoginUserQueryHandler _handler;

        public LoginUserQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthService>();
            _handler = new LoginUserQueryHandler(_userRepositoryMock.Object, _authServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = "hashedPassword",
                Role = UserRole.Admin,
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.VerifyPasswordHash(password, user.PasswordHash))
                .Returns(true);
            _authServiceMock.Setup(x => x.GenerateToken(user.Id, user.Email, user.Role))
                .Returns("validToken");

            var query = new LoginUserQuery { Email = email, Password = password };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.AccessToken.ShouldBe("validToken");
            result.Data.UserId.ShouldBe(1);
            result.Data.RoleName.ShouldBe(UserRole.Admin);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            var query = new LoginUserQuery { Email = email, Password = password };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldContain("Invalid credentials no such user");
        }

        [Fact]
        public async Task Handle_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = "hashedPassword",
                Role = UserRole.Admin,
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.VerifyPasswordHash(password, user.PasswordHash))
                .Returns(false);

            var query = new LoginUserQuery { Email = email, Password = password };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Invalid credentials");
        }

        [Fact]
        public async Task Handle_InactiveUser_ReturnsFailure()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = "hashedPassword",
                Role = UserRole.Admin,
                IsActive = false
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.VerifyPasswordHash(password, user.PasswordHash))
                .Returns(true);

            var query = new LoginUserQuery { Email = email, Password = password };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Account is inactive");
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.TeamLead)]
        [InlineData(UserRole.Trainee)]
        public async Task Handle_DifferentRoles_ReturnsSuccessWithCorrectRole(UserRole role)
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                Id = 1,
                Email = email,
                PasswordHash = "hashedPassword",
                Role = role,
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);
            _authServiceMock.Setup(x => x.VerifyPasswordHash(password, user.PasswordHash))
                .Returns(true);
            _authServiceMock.Setup(x => x.GenerateToken(user.Id, user.Email, user.Role))
                .Returns("validToken");

            var query = new LoginUserQuery { Email = email, Password = password };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Data.RoleName.ShouldBe(role);
        }
    }
}
