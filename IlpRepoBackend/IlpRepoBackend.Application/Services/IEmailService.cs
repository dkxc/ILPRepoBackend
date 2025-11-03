using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendEmailWithTemplateAsync(
            string serviceName,
            string to,
            string recipientName,
            Dictionary<string, string> templateData,
            int? relatedEntityId = null,
            string? relatedEntityType = null);
    }
}
