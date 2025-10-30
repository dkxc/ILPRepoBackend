using AutoMapper;
using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.CustomeException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Users
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByIdAsync(request.Id);
            if (existingUser == null)
                throw new NotFoundException(nameof(User), request.Id);

            // Check if username is being changed and if it already exists
            if (existingUser.Username != request.Username &&
                await _userRepository.UsernameExistsAsync(request.Username))
            {
                throw new InvalidOperationException($"Username '{request.Username}' already exists");
            }

            existingUser.Username = request.Username;
            existingUser.Role = request.Role;
            existingUser.IsActive = request.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Only update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            return _mapper.Map<UserDto>(updatedUser);
        }
    }
}
