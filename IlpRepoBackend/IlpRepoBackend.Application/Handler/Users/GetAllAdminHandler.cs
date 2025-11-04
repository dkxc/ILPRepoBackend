using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Users;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Users
{
    public class GetAllAdminHandler : IRequestHandler<GetAllAdmin, List<UserDto>>
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper _mapper;

        public GetAllAdminHandler(IUserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            _mapper = mapper;
        }

       

         async Task<List<UserDto>> IRequestHandler<GetAllAdmin, List<UserDto>>.Handle(GetAllAdmin request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();

            // Filter admins
            var admins = users
                .Where(u => u.Role == UserRole.Admin)
                .ToList();

            // mapping to DTO
            var result = admins.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive
            }).ToList();

            return result;
        }
    }
}
