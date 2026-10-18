using MDRNJ.Shared.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MDRNJ.Api.Controllers
{
    /// <summary>
    /// Public Terms of Service and Privacy Policy pages (the store listings and the app link
    /// here). The text lives in Legal/*.html; Legal:ContactEmail fills in the contact address.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("")]
    public class LegalController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LegalController(IConfiguration config) => _config = config;

        [HttpGet(LegalRules.TermsPath)]
        public IActionResult Terms() => Page("terms.html");

        [HttpGet(LegalRules.PrivacyPath)]
        public IActionResult Privacy() => Page("privacy.html");

        private IActionResult Page(string file)
        {
            using var stream = typeof(LegalController).Assembly.GetManifestResourceStream($"MDRNJ.Api.Legal.{file}")!;
            var html = new StreamReader(stream).ReadToEnd()
                .Replace("{{CONTACT_EMAIL}}", _config["Legal:ContactEmail"] ?? "contact-not-set@example.com")
                .Replace("{{VERSION}}", LegalRules.TermsVersion.ToString())
                .Replace("{{EFFECTIVE}}", LegalRules.EffectiveDate);
            return Content(html, "text/html; charset=utf-8");
        }
    }
}
