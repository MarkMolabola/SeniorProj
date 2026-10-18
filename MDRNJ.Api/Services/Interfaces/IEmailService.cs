using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string code);
        Task SendPasswordResetCodeAsync(string toEmail, string code);
    }
}
