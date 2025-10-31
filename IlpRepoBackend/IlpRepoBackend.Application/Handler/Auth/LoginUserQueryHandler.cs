using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Auth;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Auth
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, ApiResponse<LoginResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public LoginUserQueryHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<ApiResponse<LoginResponseDto>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            // Get user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.Fail("Invalid credentials no such user");
            }

            // Verify password hash
            if (!_authService.VerifyPasswordHash(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.Fail("Invalid credentials");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return ApiResponse<LoginResponseDto>.Fail("Account is inactive");
            }

            // Convert enum to string for the token
            var roleName = user.Role;

            // Generate JWT
            var token = _authService.GenerateToken(user.Id, user.Email, user.Role);

            // Prepare response
            var dto = new LoginResponseDto
            {
                AccessToken = token,
                UserId = user.Id,
                RoleName = user.Role
            };

            return ApiResponse<LoginResponseDto>.Success(dto, "Login successful");
        }
    }
}