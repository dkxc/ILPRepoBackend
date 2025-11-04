using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Auth
{
    public class InitiatePasswordSetupCommand : IRequest<InitiatePasswordSetupResponse>
    {
        public string Email { get; }

        public InitiatePasswordSetupCommand(string email)
        {
            Email = email;
        }
    }

    public class InitiatePasswordSetupResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool EmailSent { get; set; }
    }
}
