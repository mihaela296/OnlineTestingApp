using System.Net;
using System.Net.Mail;

namespace OnlineTestingApp.Services
{
    public interface IEmailService
    {
        Task SendResetCodeAsync(string toEmail, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.mail.ru";
        private readonly int _smtpPort = 465;
        private readonly string _senderEmail = "твой-email@mail.ru"; // 🔴 ЗАМЕНИ
        private readonly string _senderPassword = "твой-пароль-приложения"; // 🔴 ЗАМЕНИ

        public async Task SendResetCodeAsync(string toEmail, string code)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, "Online Testing Platform"),
                Subject = "Код для сброса пароля",
                Body = $@"
                    <h2>Сброс пароля</h2>
                    <p>Ваш код: <strong>{code}</strong></p>
                    <p>Код действителен 15 минут.</p>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
