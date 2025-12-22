

namespace Shared.Contracts.Contracts
{
    public interface IEnviarEmailAsync
    {
        Task EnviarEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }
}