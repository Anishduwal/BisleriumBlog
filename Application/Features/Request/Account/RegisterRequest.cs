namespace BisleriumBlog.Application.Features.Request.Account;

public class RegisterRequest
{
    public string? EmailID { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? MobileNumber { get; set; }
    public string? Password { get; set; }
    public string? ImagePath { get; set; }
}
