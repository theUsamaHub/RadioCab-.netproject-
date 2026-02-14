using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtp = new SmtpClient
        {
            Host = _config["EmailSettings:SmtpServer"],
            Port = int.Parse(_config["EmailSettings:Port"]),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _config["EmailSettings:Username"],
                _config["EmailSettings:Password"]
            )
        };

        var message = new MailMessage
        {
            From = new MailAddress(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:SenderName"]
            ),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        await smtp.SendMailAsync(message);
    }
}
