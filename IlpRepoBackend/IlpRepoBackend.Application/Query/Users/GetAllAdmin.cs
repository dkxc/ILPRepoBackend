using IlpRepoBackend.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Query.Users
{
    public class GetAllAdmin : IRequest<List<UserDto>>
    {
    }
}
