using IlpRepoBackend.Application.Command.Auth;
using IlpRepoBackend.Application.Handler.Auth;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Auth
{
    public class InitiatePasswordSetupCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly Mock<IPasswordSetupEmailService> _emailServiceMock;
        private readonly InitiatePasswordSetupCommandHandler _handler;

        public InitiatePasswordSetupCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _otpServiceMock = new Mock<IOtpService>();
            _emailServiceMock = new Mock<IPasswordSetupEmailService>();
            _handler = new InitiatePasswordSetupCommandHandler(
                _userRepositoryMock.Object,
                _otpServiceMock.Object,
                _emailServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidEmail_GeneratesAndSendsOtp()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = 1,
                Email = email,
                Username = "testuser"
            };
            var otp = "123456";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns(otp);
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(email, otp, user.Username))
                .ReturnsAsync(true);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.EmailSent.ShouldBeTrue();
            result.Message.ShouldBe("OTP sent to your email.");

            _otpServiceMock.Verify(x => x.GenerateOtp(), Times.Once);
            _otpServiceMock.Verify(x => x.StoreOtp(email, otp), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordSetupOtpAsync(email, otp, user.Username), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.EmailSent.ShouldBeFalse();
            result.Message.ShouldBe("User not found with this email address.");

            _otpServiceMock.Verify(x => x.GenerateOtp(), Times.Never);
            _emailServiceMock.Verify(x => x.SendPasswordSetupOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmailSendFails_ReturnsSuccessWithWarning()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = 1, Email = email, Username = "testuser" };
            var otp = "123456";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns(otp);
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(email, otp, user.Username))
                .ReturnsAsync(false);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            result.EmailSent.ShouldBeFalse();
            result.Message.ShouldBe("Failed to send OTP. Please try again.");
        }

        [Fact]
        public async Task Handle_StoresOtpCorrectly()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = 1, Email = email, Username = "testuser" };
            var otp = "654321";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns(otp);
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _otpServiceMock.Verify(x => x.StoreOtp(email, otp), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithCorrectEmail()
        {
            // Arrange
            var email = "specific@example.com";
            var user = new User { Id = 1, Email = email, Username = "testuser" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns("123456");
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task Handle_UsesUsernameInEmail()
        {
            // Arrange
            var email = "test@example.com";
            var username = "johndoe";
            var user = new User { Id = 1, Email = email, Username = username };
            var otp = "123456";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns(otp);
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(email, otp, username))
                .ReturnsAsync(true);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _emailServiceMock.Verify(x => x.SendPasswordSetupOtpAsync(email, otp, username), Times.Once);
        }

        [Fact]
        public async Task Handle_UserWithNullUsername_UsesEmail()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = 1, Email = email, Username = null };
            var otp = "123456";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns(otp);
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(email, otp, email))
                .ReturnsAsync(true);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _emailServiceMock.Verify(x => x.SendPasswordSetupOtpAsync(email, otp, email), Times.Once);
        }

        [Theory]
        [InlineData("user1@example.com")]
        [InlineData("admin@example.com")]
        [InlineData("test.user@company.co.uk")]
        public async Task Handle_DifferentEmails_ProcessesCorrectly(string email)
        {
            // Arrange
            var user = new User { Id = 1, Email = email, Username = "testuser" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.GenerateOtp())
                .Returns("123456");
            _emailServiceMock.Setup(x => x.SendPasswordSetupOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new InitiatePasswordSetupCommand(email);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        }
    }
}
