using BisleriumBlog.Application.Features.Request.Email;

namespace BisleriumBlog.Infrastructure.Interfaces.Services;

public interface IMailService
{
    void SendMail(EmailRequest email);
}
