using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string emailTo, string subject, string body);
    }
}