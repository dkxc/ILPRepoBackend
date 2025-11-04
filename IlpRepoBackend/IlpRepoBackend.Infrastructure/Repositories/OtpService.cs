using IlpRepoBackend.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class OtpService : IOtpService
    {
        private static readonly Dictionary<string, OtpData> _otpStore = new();
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(10);

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public void StoreOtp(string email, string otp, string purpose = "password_setup")
        {
            var key = $"{email}_{purpose}";
            _otpStore[key] = new OtpData
            {
                Otp = otp,
                Email = email,
                Purpose = purpose,
                Expiry = DateTime.UtcNow.Add(_otpExpiry),
                CreatedAt = DateTime.UtcNow
            };

            // Clean expired OTPs periodically
            CleanExpiredOtps();
        }

        public bool VerifyOtp(string email, string otp, string purpose = "password_setup")
        {
            var key = $"{email}_{purpose}";

            if (_otpStore.TryGetValue(key, out var otpData))
            {
                if (DateTime.UtcNow <= otpData.Expiry && otpData.Otp == otp)
                {
                    _otpStore.Remove(key);
                    return true;
                }
                _otpStore.Remove(key); // Remove expired or invalid OTP
            }
            return false;
        }

        public void CleanExpiredOtps()
        {
            var expiredKeys = _otpStore
                .Where(kvp => DateTime.UtcNow > kvp.Value.Expiry)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _otpStore.Remove(key);
            }
        }

        private class OtpData
        {
            public string Otp { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Purpose { get; set; } = "password_setup";
            public DateTime Expiry { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
