using System.Threading.Tasks;

namespace ProiectMDS.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toemail, string subject, string body);
    }
}