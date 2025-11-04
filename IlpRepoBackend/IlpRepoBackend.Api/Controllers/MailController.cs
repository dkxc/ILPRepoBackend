using IlpRepoBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public MailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        // ✅ Send normal email
        [HttpPost("send-plain")]
        public async Task<IActionResult> SendEmail([FromBody] PlainEmailRequest request)
        {
            var result = await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);

            if (result)
                return Ok(new { message = "Email sent successfully" });

            return BadRequest(new { message = "Failed to send email" });
        }

        // ✅ Send email using template config from DB
        [HttpPost("send-template")]
        public async Task<IActionResult> SendWithTemplate([FromBody] TemplateEmailRequest request)
        {
            var result = await _emailService.SendEmailWithTemplateAsync(
                request.ServiceName,
                request.To,
                request.RecipientName,
                request.TemplateData,
                request.RelatedEntityId,
                request.RelatedEntityType
            );

            if (result)
                return Ok(new { message = "Template email sent successfully" });

            return BadRequest(new { message = "Failed to send template email" });
        }
    }

    // 📩 Request Models
    public class PlainEmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class TemplateEmailRequest
    {
        public string ServiceName { get; set; }
        public string To { get; set; }
        public string RecipientName { get; set; }
        public Dictionary<string, string> TemplateData { get; set; }
        public int? RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
    }
}
