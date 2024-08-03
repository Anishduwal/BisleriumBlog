namespace BisleriumBlog.Application.Features.Request.Profile;

public class ChangePasswordRequest
{
    public string NewPassword { get; set; }

    public string ConfirmPassword { get; set; }

    public string CurrentPassword { get; set; }
}
