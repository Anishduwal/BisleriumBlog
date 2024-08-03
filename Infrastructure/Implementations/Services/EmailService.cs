using System.Net.Mail;
using System.Net;
using BisleriumBlog.Application.Features.DTOs.Email;
using BisleriumBlog.Infrastructure.Interfaces.Services;

namespace BisleriumBlog.Infrastructure.Implementations.Services;

public class EmailService : IEmailService
{
    public void SendEmail(EmailDto email)
    {
        try
        {
            const string fromMail = "np01cp4s220147@islingtoncollege.edu.np";

            const string fromPassword = "fjhr obxw ojck pkfb";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromMail),
                Subject = email.Subject,
                Body = "<html><body> " + email.Message + " </body></html>",
                IsBodyHtml = true,
            };

            message.To.Add(new MailAddress(email.Email));

            smtpClient.Send(message);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"SMTP Exception: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
        }
    }
}