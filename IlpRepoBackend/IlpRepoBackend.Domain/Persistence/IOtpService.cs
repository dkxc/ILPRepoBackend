using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IOtpService
    {
        string GenerateOtp();
        void StoreOtp(string email, string otp, string purpose = "password_setup");
        bool VerifyOtp(string email, string otp, string purpose = "password_setup");
        void CleanExpiredOtps();
    }
}
