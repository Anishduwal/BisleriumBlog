using System.ComponentModel.DataAnnotations;

namespace BisleriumBlog.Application.Features.Request.Account;

public class LoginRequest
{
    [EmailAddress]
    [Required]
    public string EmailID { get; set; }

    [Required]
    public string Password { get; set; }
}
