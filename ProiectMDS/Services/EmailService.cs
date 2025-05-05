using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using ProiectMDS.Services;

namespace ProiectMDS.Services
{
    public class EmailService : IEmailSender, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _senderEmail = "proiect778@gmail.com";
        private readonly string _senderPassword = "oktpaturufmzgodn";

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mail = new MailMessage();
            var myEmail = "proiect778@gmail.com";
            mail.From = new MailAddress(_senderEmail);
            mail.To.Add(myEmail);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                smtp.EnableSsl = true;

                //await smtp.SendMailAsync(mail);
                try
                {
                    await smtp.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Eroare la trimiterea emailului: " + ex.Message);
                }
            }
        }
    }

}
