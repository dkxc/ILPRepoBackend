using IlpRepoBackend.Application.Command.Auth;
using IlpRepoBackend.Application.Handler.Auth;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Auth
{
    public class VerifyOtpCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly VerifyOtpCommandHandler _handler;

        public VerifyOtpCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthService>();
            _otpServiceMock = new Mock<IOtpService>();
            _handler = new VerifyOtpCommandHandler(
                _userRepositoryMock.Object,
                _authServiceMock.Object,
                _otpServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidOtp_ReturnsSuccessWithToken()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "123456";
            var userId = 1;
            var token = "password-setup-token";

            var user = new User
            {
                Id = userId,
                Email = email,
                Username = "testuser"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(true);
            _authServiceMock.Setup(x => x.GeneratePasswordSetupToken(userId, email))
                .Returns(token);

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Succeeded.ShouldBeTrue();
            result.Message.ShouldBe("OTP verified successfully.");
            result.Token.ShouldBe(token);
            result.UserId.ShouldBe(userId);

            _otpServiceMock.Verify(x => x.VerifyOtp(email, otp), Times.Once);
            _authServiceMock.Verify(x => x.GeneratePasswordSetupToken(userId, email), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var otp = "123456";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("User not found.");
            result.Token.ShouldBeEmpty();

            _otpServiceMock.Verify(x => x.VerifyOtp(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _authServiceMock.Verify(x => x.GeneratePasswordSetupToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidOtp_ReturnsFailure()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "wrong-otp";
            var user = new User { Id = 1, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(false);

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Invalid or expired OTP.");
            result.Token.ShouldBeEmpty();

            _authServiceMock.Verify(x => x.GeneratePasswordSetupToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExpiredOtp_ReturnsFailure()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "123456";
            var user = new User { Id = 1, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(false); // OTP expired

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeFalse();
            result.Message.ShouldBe("Invalid or expired OTP.");
        }

        [Fact]
        public async Task Handle_GeneratesTokenWithCorrectParameters()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "123456";
            var userId = 5;

            var user = new User { Id = userId, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(true);
            _authServiceMock.Setup(x => x.GeneratePasswordSetupToken(userId, email))
                .Returns("token");

            var command = new VerifyOtpCommand(email, otp);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _authServiceMock.Verify(x => x.GeneratePasswordSetupToken(userId, email), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectUserId()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "123456";
            var userId = 42;

            var user = new User { Id = userId, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(true);
            _authServiceMock.Setup(x => x.GeneratePasswordSetupToken(userId, email))
                .Returns("token");

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.UserId.ShouldBe(userId);
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("654321")]
        [InlineData("999999")]
        public async Task Handle_DifferentValidOtps_ReturnsSuccess(string otp)
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = 1, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(true);
            _authServiceMock.Setup(x => x.GeneratePasswordSetupToken(It.IsAny<int>(), It.IsAny<string>()))
                .Returns("token");

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _otpServiceMock.Verify(x => x.VerifyOtp(email, otp), Times.Once);
        }

        [Fact]
        public async Task Handle_ChecksUserExistenceBeforeVerifyingOtp()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "123456";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            var command = new VerifyOtpCommand(email, otp);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _otpServiceMock.Verify(x => x.VerifyOtp(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TokenNotGeneratedForInvalidOtp()
        {
            // Arrange
            var email = "test@example.com";
            var otp = "invalid";
            var user = new User { Id = 1, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(false);

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Token.ShouldBeEmpty();
            _authServiceMock.Verify(x => x.GeneratePasswordSetupToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("user1@example.com")]
        [InlineData("admin@company.com")]
        [InlineData("test.user@example.co.uk")]
        public async Task Handle_DifferentEmails_ProcessesCorrectly(string email)
        {
            // Arrange
            var otp = "123456";
            var user = new User { Id = 1, Email = email };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);
            _otpServiceMock.Setup(x => x.VerifyOtp(email, otp))
                .Returns(true);
            _authServiceMock.Setup(x => x.GeneratePasswordSetupToken(It.IsAny<int>(), email))
                .Returns("token");

            var command = new VerifyOtpCommand(email, otp);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.ShouldBeTrue();
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _authServiceMock.Verify(x => x.GeneratePasswordSetupToken(It.IsAny<int>(), email), Times.Once);
        }
    }
}
