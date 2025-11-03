using AutoMapper;
using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Users
{
    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public UpdatePasswordCommandHandler(
            IUserRepository userRepository,
            IMapper mapper,
            IAuthService authService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<UserDto> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            // Get the user
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
            }

            // Verify current password
            if (!_authService.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Update password
            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDto>(updatedUser);
        }
    }
}