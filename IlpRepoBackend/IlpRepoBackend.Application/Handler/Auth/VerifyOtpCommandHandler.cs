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
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, VerifyOtpResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IOtpService _otpService;

        public VerifyOtpCommandHandler(
            IUserRepository userRepository,
            IAuthService authService,
            IOtpService otpService)
        {
            _userRepository = userRepository;
            _authService = authService;
            _otpService = otpService;
        }

        public async Task<VerifyOtpResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                return new VerifyOtpResponse
                {
                    Succeeded = false,
                    Message = "User not found."
                };
            }

            if (!_otpService.VerifyOtp(request.Email, request.Otp))
            {
                return new VerifyOtpResponse
                {
                    Succeeded = false,
                    Message = "Invalid or expired OTP."
                };
            }

            // Generate password setup token
            var token = _authService.GeneratePasswordSetupToken(user.Id, user.Email);

            return new VerifyOtpResponse
            {
                Succeeded = true,
                Message = "OTP verified successfully.",
                Token = token,
                UserId = user.Id
            };
        }
    }
}
