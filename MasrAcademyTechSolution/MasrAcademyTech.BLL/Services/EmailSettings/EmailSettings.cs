using System.Net;
using System.Net.Mail;

namespace MasrAcademyTech.BLL.Services.EmailSettings
{
    public class EmailSettings : IEmailSettings
    {
        public void SendEmail(Email email)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("fadywageih14@gmail.com", "xdsgrgmujalnoupt")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("fadywageih14@gmail.com", "MasrAcademyTech"),
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email.To);

            client.Send(mailMessage);
        }
    }
}