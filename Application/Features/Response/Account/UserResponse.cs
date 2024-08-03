namespace BisleriumBlog.Application.Features.Response.Account;

public class UserResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? UserName { get; set; }
    public string? EmailID { get; set; }
    public string? Role { get; set; }
    public int RoleId { get; set; }
    public string? Token { get; set; }
    public string? ImagePath { get; set; }
}
