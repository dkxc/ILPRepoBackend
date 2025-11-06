using IlpRepoBackend.Application.Handler.Auth;
using IlpRepoBackend.Application.Query.Auth;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Auth
{
    public class ValidateTokenQueryHandlerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly ValidateTokenQueryHandler _handler;

        public ValidateTokenQueryHandlerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _handler = new ValidateTokenQueryHandler(_authServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidToken_ReturnsTrue()
        {
            // Arrange
            var token = "valid.jwt.token";
            _authServiceMock.Setup(x => x.ValidateToken(token))
                .Returns(true);

            var query = new ValidateTokenQuery { Token = token };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var token = "invalid.jwt.token";
            _authServiceMock.Setup(x => x.ValidateToken(token))
                .Returns(false);

            var query = new ValidateTokenQuery { Token = token };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_EmptyToken_ReturnsFalse()
        {
            // Arrange
            var token = "";
            _authServiceMock.Setup(x => x.ValidateToken(token))
                .Returns(false);

            var query = new ValidateTokenQuery { Token = token };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_NullToken_ReturnsFalse()
        {
            // Arrange
            string? token = null;
            _authServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>()))
                .Returns(false);

            var query = new ValidateTokenQuery { Token = token };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
        }

        [Theory]
        [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.validtoken")]
        [InlineData("Bearer token123")]
        [InlineData("random_token_string")]
        public async Task Handle_VariousTokenFormats_CallsAuthService(string token)
        {
            // Arrange
            _authServiceMock.Setup(x => x.ValidateToken(token))
                .Returns(true);

            var query = new ValidateTokenQuery { Token = token };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _authServiceMock.Verify(x => x.ValidateToken(token), Times.Once);
        }
    }
}
