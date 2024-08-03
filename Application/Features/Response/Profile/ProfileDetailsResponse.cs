namespace BisleriumBlog.Application.Features.Response.Profile;

public class ProfileDetailsResponse
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string FullName { get; set; }
    public string ContactNumber { get; set; }
    public string UserName { get; set; }
    public string EmailID { get; set; }
    public string RoleName { get; set; }
    public string? ImagePath { get; set; }
}
