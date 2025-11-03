using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Query.Auth
{
    public class ValidateTokenQuery : IRequest<bool>
    {
        public string Token { get; set; }
    }

}
