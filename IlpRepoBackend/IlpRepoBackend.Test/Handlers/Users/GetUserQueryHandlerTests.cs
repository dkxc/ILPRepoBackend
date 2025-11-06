using AutoMapper;
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
    public class GetUserQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUserQueryHandler _handler;

        public GetUserQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetUserQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_UsersExist_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "user1@example.com", Role = UserRole.Admin, IsActive = true },
                new User { Id = 2, Username = "user2", Email = "user2@example.com", Role = UserRole.Trainee, IsActive = true },
                new User { Id = 3, Username = "user3", Email = "user3@example.com", Role = UserRole.TeamLead, IsActive = false }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "user1", Email = "user1@example.com", Role = UserRole.Admin, IsActive = true },
                new UserDto { Id = 2, Username = "user2", Email = "user2@example.com", Role = UserRole.Trainee, IsActive = true },
                new UserDto { Id = 3, Username = "user3", Email = "user3@example.com", Role = UserRole.TeamLead, IsActive = false }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _mapperMock.Setup(x => x.Map<List<UserDto>>(users))
                .Returns(userDtos);

            var query = new GetUserQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result[0].Username.ShouldBe("user1");
            result[1].Username.ShouldBe("user2");
            result[2].Username.ShouldBe("user3");
        }

        [Fact]
        public async Task Handle_NoUsers_ReturnsEmptyList()
        {
            // Arrange
            var users = new List<User>();
            var userDtos = new List<UserDto>();

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _mapperMock.Setup(x => x.Map<List<UserDto>>(users))
                .Returns(userDtos);

            var query = new GetUserQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Handle_UsersWithDifferentRoles_ReturnsAll()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Role = UserRole.Admin },
                new User { Id = 2, Role = UserRole.TeamLead },
                new User { Id = 3, Role = UserRole.Trainee }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Role = UserRole.Admin },
                new UserDto { Id = 2, Role = UserRole.TeamLead },
                new UserDto { Id = 3, Role = UserRole.Trainee }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _mapperMock.Setup(x => x.Map<List<UserDto>>(users))
                .Returns(userDtos);

            var query = new GetUserQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldContain(u => u.Role == UserRole.Admin);
            result.ShouldContain(u => u.Role == UserRole.TeamLead);
            result.ShouldContain(u => u.Role == UserRole.Trainee);
        }

        [Fact]
        public async Task Handle_ActiveAndInactiveUsers_ReturnsAll()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "active", IsActive = true },
                new User { Id = 2, Username = "inactive", IsActive = false }
            };

            var userDtos = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "active", IsActive = true },
                new UserDto { Id = 2, Username = "inactive", IsActive = false }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _mapperMock.Setup(x => x.Map<List<UserDto>>(users))
                .Returns(userDtos);

            var query = new GetUserQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldContain(u => u.IsActive);
            result.ShouldContain(u => !u.IsActive);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<User>());
            _mapperMock.Setup(x => x.Map<List<UserDto>>(It.IsAny<List<User>>()))
                .Returns(new List<UserDto>());

            var query = new GetUserQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }
    }
}
