using BisleriumBlog.Application.Features.Response.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BisleriumBlog.Application.Models;
using BisleriumBlog.Application.Utilities;
using BisleriumBlog.Application.Features.Request.Account;
using BisleriumBlog.Application.Features.Response.Dashboard;
using BisleriumBlog.Application.Features.Response.User;
using BisleriumBlog.Infrastructure.Persistence;

namespace BisleriumBlog.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _DBContext;

    public AdminController(ApplicationDbContext dbContext)
    {
        _DBContext = dbContext;
    }

    [HttpGet("get-users-list")]
    public IActionResult GetAllUsers()
    {
        var users = _DBContext.Users.ToList();
        var userDetails = new List<UserDetailResponse>();

        if (users.Any())  // If there are users in the list
        {
            foreach (var user in users)
            {
                var role = _DBContext.Roles.Find(user.RoleId);
                var userDetail = new UserDetailResponse
                {
                    ID = user.Id,
                    Name = user.FullName,
                    RoleID = user.RoleId,
                    EmailID = user.EmailAddress,
                    ImagePath = string.IsNullOrEmpty(user.ImagePath) ? "Sample.svg" : user.ImagePath,
                    UserName = user.UserName,
                    RoleName = role?.Name ?? "Unknown"
                };

                userDetails.Add(userDetail);
            }

            // Return response with user details
            return Ok(new ResponseModel<List<UserDetailResponse>>
            {
                Message = "Successfully Retrieved Users",
                Result = userDetails,
                Status = "Success",
                StatusCode = HttpStatusCode.OK,
                TotalCount = userDetails.Count
            });
        }
        else
        {
            // In case there are no users in the list
            return Ok(new ResponseModel<List<UserDetailResponse>>
            {
                Message = "There is no user list.",
                Result = userDetails,
                Status = "Success",
                StatusCode = HttpStatusCode.OK,
                TotalCount = 0
            });
        }
    }


    [HttpPost("create-admin")]
    public IActionResult RegisterAdministrator(RegisterRequest register)
    {
        // Check if a user with the same email or username already exists
        var existingUser = _DBContext.Users
            .FirstOrDefault(x => x.EmailAddress == register.EmailID || x.UserName == register.UserName);

        if (existingUser == null)
        {
            // Retrieve the admin role from the database
            var adminRole = _DBContext.Roles.FirstOrDefault(x => x.Name == "AdminUser");

            if (adminRole == null)
            {
                return BadRequest(new ResponseModel<bool>
                {
                    Message = "Admin role not found",
                    Result = false,
                    Status = "Bad Request",
                    StatusCode = HttpStatusCode.BadRequest,
                    TotalCount = 0
                });
            }

            // Create a new user with the admin role
            var newUser = new User
            {
                FullName = register.FullName,
                EmailAddress = register.EmailID,
                RoleId = adminRole.Id,
                Password = PasswordManager.GenerateHash("Admin@101"),
                UserName = register.UserName,
                ContactNo = register.MobileNumber,
                ImagePath = register.ImagePath
            };

            // Add the new user to the database
            _DBContext.Users.Add(newUser);
            _DBContext.SaveChanges();

            // Return a successful response
            return Ok(new ResponseModel<object>
            {
                Message = "Successfully registered",
                Result = true,
                Status = "Success",
                StatusCode = HttpStatusCode.OK,
                TotalCount = 1
            });
        }

        // Return a bad request response if the user already exists
        return BadRequest(new ResponseModel<bool>
        {
            Message = "Existing user with the same user name or email address",
            Result = false,
            Status = "Bad Request",
            StatusCode = HttpStatusCode.BadRequest,
            TotalCount = 0
        });
    }

    /*This method fetches dashboard details, including post count, comment count, upvotes, 
    downvotes, and blog popularity, and returns them in a response.*/

    [HttpGet("dashboard-details")]
    public IActionResult GetDashboardDetails()
    {
        // Retrieve active data from the database
        var activeBlogs = _DBContext.Blogs.Where(b => b.IsActive).ToList();
        var activeReactions = _DBContext.Reactions.Where(r => r.IsActive).ToList();
        var activeComments = _DBContext.Comments.Where(c => c.IsActive).ToList();

        // Calculate dashboard details
        var dashboardCount = new DashboardCount
        {
            Posts = activeBlogs.Count,
            Comments = activeComments.Count,
            UpVotes = activeReactions.Count(r => r.ReactionId == 1),
            DownVotes = activeReactions.Count(r => r.ReactionId == 2)
        };

        var blogDetailsList = activeBlogs.Select(blog =>
        {
            var upVotes = activeReactions.Count(r => r.ReactionId == 1 && r.BlogId == blog.Id && r.IsReactedForBlog);
            var downVotes = activeReactions.Count(r => r.ReactionId == 2 && r.BlogId == blog.Id && r.IsReactedForBlog);
            var commentReactions = activeComments.Count(c => c.BlogId == blog.Id && c.IsCommentForBlog);
            var commentForComments = activeComments.Count(c => activeComments.Any(ac => ac.CommentId == c.CommentId && ac.IsCommentForComment));

            var popularity = upVotes * 2 - downVotes + commentReactions + commentForComments;

            return new BlogDetails
            {
                BlogId = blog.Id,
                Blog = blog.Title,
                BloggerId = blog.CreatedById,
                Popularity = popularity
            };
        }).ToList();

        var bloggerDetailsList = blogDetailsList
            .GroupBy(blog => blog.BloggerId)
            .Select(group => new BloggerDetails
            {
                BloggerId = group.Key,
                BloggerName = _DBContext.Users.Find(group.Key)?.FullName ?? "Unknown",
                Popularity = group.Sum(blog => blog.Popularity)
            }).ToList();

        var popularBlogs = blogDetailsList
            .OrderByDescending(b => b.Popularity)
            .Take(10)
            .Select(blog => new FamousBlog
            {
                BlogId = blog.BlogId,
                Blog = blog.Blog
            }).ToList();

        var popularBloggers = bloggerDetailsList
            .OrderByDescending(b => b.Popularity)
            .Take(10)
            .Select(blogger => new FamousBlogger
            {
                BloggerId = blogger.BloggerId,
                BloggerName = blogger.BloggerName
            }).ToList();

        var dashboardDetails = new DashboardDetailsResponse
        {
            DashboardCount = dashboardCount,
            FamousBloggers = popularBloggers,
            FamousBlogs = popularBlogs
        };

        var response = new ResponseModel<DashboardDetailsResponse>
        {
            Message = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Result = dashboardDetails,
            Status = "Success"
        };

        return Ok(response);
    }

}
