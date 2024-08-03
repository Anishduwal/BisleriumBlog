namespace BisleriumBlog.Application.Features.Response.User;

public class UserDetailResponse
{
    public int ID { get; set; }

    public int RoleID { get; set; }

    public string EmailID { get; set; }

    public string UserName { get; set; }

    public string Name { get; set; }

    public string RoleName { get; set; }

    public string ImagePath { get; set; }
}
