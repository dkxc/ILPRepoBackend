using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Auth
{
    public class SetPasswordCommand : IRequest<SetPasswordResponse>
    {
        public string Email { get; } // Add email to the command
        public string NewPassword { get; }
        public string Token { get; }

        public SetPasswordCommand(string email, string newPassword, string token)
        {
            Email = email;
            NewPassword = newPassword;
            Token = token;
        }
    }

    public class SetPasswordResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
