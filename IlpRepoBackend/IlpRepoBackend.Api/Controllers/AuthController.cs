using IlpRepoBackend.Application.Command.Auth;
using IlpRepoBackend.Application.Command.SignInDto;
using IlpRepoBackend.Application.Query.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IlpRepoBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var query = new LoginUserQuery(request.Email, request.Password);
            var response = await _mediator.Send(query);

            if (!response.Succeeded)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }
        [HttpPost("initiate-password-setup")]
        public async Task<IActionResult> InitiatePasswordSetup([FromBody] InitiatePasswordSetupDto request)
        {
            //_logger.LogInformation("Initiate password setup request for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                //_logger.LogWarning("Invalid model state for initiate password setup");
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var command = new InitiatePasswordSetupCommand(request.Email);
                var response = await _mediator.Send(command);

                if (!response.Succeeded)
                {
                    //_logger.LogWarning("Password setup initiation failed for {Email}: {Message}", request.Email, response.Message);
                    return BadRequest(new
                    {
                        success = false,
                        message = response.Message,
                        emailSent = response.EmailSent
                    });
                }

                //_logger.LogInformation("Password setup initiated successfully for {Email}", request.Email);
                return Ok(new
                {
                    success = true,
                    message = response.Message,
                    emailSent = response.EmailSent
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in initiate-password-setup endpoint for email: {Email}", request.Email);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An internal server error occurred. Please try again later."
                });
            }
        }


        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenQuery query)
        {
            var result = await _mediator.Send(query);

            if (!result)
                return Unauthorized(new { message = "Invalid or expired token" });

            return Ok(new { message = "Valid token" });
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new VerifyOtpCommand(request.Email, request.Otp);
            var response = await _mediator.Send(command);

            if (!response.Succeeded)
            {
                return Unauthorized(new { message = response.Message });
            }

            return Ok(new
            {
                message = response.Message,
                token = response.Token,
                userId = response.UserId
            });
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto request, [FromHeader] string authorization)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // Extract token from header
            var token = authorization?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token is required." });
            }

            // You'll need to get the email from the request or token
            // For now, let's assume it's passed in the request body
            var command = new SetPasswordCommand(request.Email, request.NewPassword, token);
            var response = await _mediator.Send(command);

            if (!response.Succeeded)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(new { message = response.Message });
        }
    }
}