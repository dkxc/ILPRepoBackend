using AutoMapper;
using IlpRepoBackend.Application.CustomeException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Handler.Users;
using IlpRepoBackend.Application.Query.Users;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using Moq;
using Shouldly;
using Xunit;

namespace IlpRepoBackend.Test.Handlers.Users
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUserByIdQueryHandler _handler;

        public GetUserByIdQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_ReturnsUser()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                Role = UserRole.Admin,
                IsActive = true
            };

            var userDto = new UserDto
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                Role = UserRole.Admin,
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

            var query = new GetUserByIdQuery { Id = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(userId);
            result.Username.ShouldBe("testuser");
            result.Email.ShouldBe("test@example.com");
            result.Role.ShouldBe(UserRole.Admin);
            result.IsActive.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = 999;
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var query = new GetUserByIdQuery { Id = userId };

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public async Task Handle_VariousUserIds_CallsRepositoryWithCorrectId(int userId)
        {
            // Arrange
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(new UserDto { Id = userId });

            var query = new GetUserByIdQuery { Id = userId };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.TeamLead)]
        [InlineData(UserRole.Trainee)]
        public async Task Handle_UsersWithDifferentRoles_ReturnsCorrectly(UserRole role)
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = role
            };

            var userDto = new UserDto
            {
                Id = 1,
                Role = role
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);
            _mapperMock.Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

            var query = new GetUserByIdQuery { Id = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Role.ShouldBe(role);
        }
    }
}
