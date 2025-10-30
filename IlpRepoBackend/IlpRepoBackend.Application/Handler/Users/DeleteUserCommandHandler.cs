using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.CustomeException;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Users
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var exists = await _userRepository.ExistsAsync(request.Id);
            if (!exists)
                throw new NotFoundException(nameof(User), request.Id);

            return await _userRepository.DeleteAsync(request.Id);
        }
    }
}
