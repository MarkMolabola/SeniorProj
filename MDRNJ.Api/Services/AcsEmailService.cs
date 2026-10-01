using Azure;
using Azure.Communication.Email;
using MDRNJ.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    // Sends email through Azure Communication Services.
    // Requires Email:ConnectionString and Email:SenderAddress.
    public class AcsEmailService : IEmailService
    {
        private readonly EmailClient _client;
        private readonly string _sender;

        public AcsEmailService(IConfiguration config)
        {
            _client = new EmailClient(config["Email:ConnectionString"]);
            _sender = config["Email:SenderAddress"]!;
        }

        public async Task SendVerificationCodeAsync(string toEmail, string code)
        {
            var subject = $"{code} is your MDRNJ Fitness verification code";
            var plainText =
                $"Your MDRNJ Fitness verification code is {code}.\n\n" +
                $"It expires in {AuthService.VerificationCodeLifetimeMinutes} minutes. " +
                "If you didn't create an account, you can ignore this email.";
            var html = $"""
                <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto">
                  <h2>Welcome to MDRNJ Fitness!</h2>
                  <p>Enter this code in the app to verify your email:</p>
                  <p style="font-size:32px;font-weight:bold;letter-spacing:8px">{code}</p>
                  <p>It expires in {AuthService.VerificationCodeLifetimeMinutes} minutes.</p>
                  <p style="color:#888;font-size:12px">If you didn't create an account, you can ignore this email.</p>
                </div>
                """;

            // WaitUntil.Started returns once ACS accepts the message instead of waiting for delivery.
            await _client.SendAsync(WaitUntil.Started, _sender, toEmail, subject, html, plainText);
        }
    }
}
