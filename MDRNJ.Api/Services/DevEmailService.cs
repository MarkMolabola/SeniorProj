using MDRNJ.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    // Used when Email:ConnectionString isn't configured (local dev).
    // Writes the code to the API console instead of sending an email.
    public class DevEmailService : IEmailService
    {
        private readonly ILogger<DevEmailService> _logger;

        public DevEmailService(ILogger<DevEmailService> logger) => _logger = logger;

        public Task SendVerificationCodeAsync(string toEmail, string code)
        {
            _logger.LogWarning("[DEV EMAIL] Verification code for {Email}: {Code}", toEmail, code);
            return Task.CompletedTask;
        }
    }
}
