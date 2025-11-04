using IlpRepoBackend.Application.Command.Auth;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Auth
{
    public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, SetPasswordResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public SetPasswordCommandHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<SetPasswordResponse> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
        {
            // Validate token and get userId
            if (!_authService.ValidatePasswordSetupToken(request.Token, out int userId))
            {
                return new SetPasswordResponse
                {
                    Succeeded = false,
                    Message = "Invalid or expired token."
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Email != request.Email)
            {
                return new SetPasswordResponse
                {
                    Succeeded = false,
                    Message = "User not found or email mismatch."
                };
            }

            // Hash and set new password
            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return new SetPasswordResponse
            {
                Succeeded = true,
                Message = "Password set successfully."
            };
        }
    }
}
