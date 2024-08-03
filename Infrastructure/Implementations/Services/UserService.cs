using BisleriumBlog.Infrastructure.Interfaces.Services;
using System.Security.Claims;

namespace BisleriumBlog.Infrastructure.Implementations.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public int UserId
    {
        get
        {
            var userIdValue = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int UserID = int.TryParse(userIdValue, out var userId) ? userId : 0;
            return UserID;
        }
    }
}