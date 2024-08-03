using BisleriumBlog.Application.Features.DTOs.Email;

namespace BisleriumBlog.Infrastructure.Interfaces.Services;

public interface IEmailService
{
    void SendEmail(EmailDto email);
}
