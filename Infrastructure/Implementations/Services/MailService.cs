using System.Net.Mail;
using System.Net;
using BisleriumBlog.Infrastructure.Interfaces.Services;
using BisleriumBlog.Application.Features.Request.Email;

namespace BisleriumBlog.Infrastructure.Implementations.Services;

public class MailService : IMailService
{
    public void SendMail(EmailRequest email)
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

            MailMessage mailMSG = new MailMessage();
            mailMSG.From = new MailAddress(fromMail);
            mailMSG.Subject = email.Subject;
            mailMSG.Body = "<html><body> " + email.Message + " </body></html>";
            mailMSG.IsBodyHtml = true;

            mailMSG.To.Add(new MailAddress(email.EmailID));

            smtpClient.Send(mailMSG);
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