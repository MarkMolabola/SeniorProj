using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string code);
    }
}
