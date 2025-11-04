using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Auth
{
    public class VerifyOtpCommand : IRequest<VerifyOtpResponse>
    {
        public string Email { get; }
        public string Otp { get; }

        public VerifyOtpCommand(string email, string otp)
        {
            Email = email;
            Otp = otp;
        }
    }

    public class VerifyOtpResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
