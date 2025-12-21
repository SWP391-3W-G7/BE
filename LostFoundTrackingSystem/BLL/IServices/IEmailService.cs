using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IEmailService
    {
        System.Threading.Tasks.Task SendEmailAsync(string to, string subject, string body);
    }
}
