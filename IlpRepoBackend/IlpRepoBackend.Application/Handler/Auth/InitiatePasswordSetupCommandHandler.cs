
using IlpRepoBackend.Application.Command.Auth;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Auth
{
    public class InitiatePasswordSetupCommandHandler : IRequestHandler<InitiatePasswordSetupCommand, InitiatePasswordSetupResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpService _otpService;
        private readonly IPasswordSetupEmailService _emailService;

        public InitiatePasswordSetupCommandHandler(
            IUserRepository userRepository,
            IOtpService otpService,
            IPasswordSetupEmailService emailService)
        {
            _userRepository = userRepository;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<InitiatePasswordSetupResponse> Handle(InitiatePasswordSetupCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                return new InitiatePasswordSetupResponse
                {
                    Succeeded = false,
                    Message = "User not found with this email address.",
                    EmailSent = false
                };
            }

            // Check if user already has a password set
            //if (string.IsNullOrEmpty(user.PasswordHash))
            //{
            //    return new InitiatePasswordSetupResponse
            //    {
            //        Succeeded = false,
            //        Message = "Password already set for this user. Use forgot password if needed.",
            //        EmailSent = false
            //    };
            //}

            // Generate and send OTP
            var otp = _otpService.GenerateOtp();
            _otpService.StoreOtp(request.Email, otp);

            var emailSent = await _emailService.SendPasswordSetupOtpAsync(
                request.Email,
                otp,
                user.Username ?? user.Email
            );

            return new InitiatePasswordSetupResponse
            {
                Succeeded = true,
                Message = emailSent ? "OTP sent to your email." : "Failed to send OTP. Please try again.",
                EmailSent = emailSent
            };
        }
    }
}
