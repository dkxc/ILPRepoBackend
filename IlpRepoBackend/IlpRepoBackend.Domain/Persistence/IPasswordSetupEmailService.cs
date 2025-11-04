using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IPasswordSetupEmailService
    {
        Task<bool> SendPasswordSetupOtpAsync(string email, string otp, string userName);
    }
}
