
using System.Net;
using System.Net.Mail;

namespace ECommerce.Utiltie
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("wm1336562@gmail.com", "icje bepn pckg dsgr")
            };
            return client.SendMailAsync(new MailMessage
                (
                 from: "wm1336562@gmail.com",
                 to: email,
                 subject,
                 htmlMessage
                )
            {
                IsBodyHtml = true
            });

        }
    }
}
