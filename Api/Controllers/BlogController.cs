using Azure;
using BisleriumBlog.Application.Models;
using BisleriumBlog.Application.Features.Response.Base;
using BisleriumBlog.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BisleriumBlog.Application.Features.Request.Blog;
using BisleriumBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BisleriumBlog.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/blog")]
public class BlogController : Controller
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _DBContext;

    public BlogController(ApplicationDbContext dbContext, IUserService userService)
    {
        _DBContext = dbContext;
        _userService = userService;
    }


    /*This method adds a new blog post, assign with respective user, and returns a response datas per success and error cases.*/

    [HttpPost("add-blog")]
    public IActionResult AddBlog(BlogCreateRequest blogRequest)
    {
        // Get the current user's ID from the user service
        var userId = _userService.UserId;

        // Fetch the user from the database
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            // Return a 404 Not Found response if the user does not exist
            return NotFound(new ResponseModel<bool>
            {
                Message = "User not found",
                Result = false,
                Status = "Not Found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0
            });
        }

        // Create the blog model
        var blogModel = new Blog
        {
            Title = blogRequest.BlogTitle,
            Body = blogRequest.Body,
            Location = blogRequest.Address,
            Reaction = blogRequest.Reactions,
            BlogImages = blogRequest.Images?.Select(imageUrl => new BlogImage
            {
                ImagePath = imageUrl,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedById = user.Id
            }).ToList(),
            CreatedDate = DateTime.UtcNow,
            CreatedById = user.Id,
        };

        // Add the blog to the database and save changes
        _DBContext.Blogs.Add(blogModel);
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Message = "Successfully inserted",
            Result = true
        });
    }


    /*This method updates an existing blog post, logs the previous state, and returns a success response upon completion*/
    [HttpPatch("update-blog")]
    public IActionResult UpdateBlog(BlogDetailsRequest blogRequest)
    {
        // Retrieve the current user's ID
        var userId = _userService.UserId;

        // Fetch the current user from the database
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            return NotFound(new ResponseModel<bool>
            {
                Message = "User not found",
                Result = false,
                Status = "Not Found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0
            });
        }

        // Fetch the blog post to be updated from the database
        var blogModel = _DBContext.Blogs.Find(blogRequest.Id);

        if (blogModel == null)
        {
            return NotFound(new ResponseModel<bool>
            {
                Message = "Blog post not found",
                Result = false,
                Status = "Not Found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0
            });
        }

        // Create a log entry for the update
        var blogLog = new BlogLog
        {
            BlogId = blogModel.Id,
            Title = blogModel.Title,
            Location = blogModel.Location,
            Reaction = blogModel.Reaction,
            CreatedDate = DateTime.UtcNow,
            CreatedById = user.Id,
            Body = blogModel.Body,
            IsActive = false
        };

        _DBContext.BlogLogs.Add(blogLog);

        // Update the blog post details
        blogModel.Title = blogRequest.BlogTitle;
        blogModel.Body = blogRequest.Body;
        blogModel.Location = blogRequest.Address;
        blogModel.Reaction = blogRequest.Reactions;
        blogModel.LastUpdatedDate = DateTime.UtcNow;
        blogModel.LastUpdatedById = user.Id;

        // Save the updated blog post to the database
        _DBContext.Blogs.Update(blogModel);
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Message = "Successfully updated",
            Result = true
        });
    }


    /*This method performs a soft delete of a blog post by updating its status, recording the deletion details, 
    and returns a success response upon completion*/

    [HttpDelete("delete-blog/{blogId:int}")]
    public IActionResult RemovePost(int blogId)
    {
        // Retrieve the current user's ID from the user service
        var userId = _userService.UserId;

        // Fetch the current user details from the database
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            // Return a 404 Not Found response if the user is not found
            return NotFound(new ResponseModel<bool>
            {
                Message = "User not found",
                Result = false,
                Status = "Not Found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0
            });
        }

        // Retrieve the blog post to be deleted from the database
        var blogModel = _DBContext.Blogs.Find(blogId);

        if (blogModel == null)
        {
            // Return a 404 Not Found response if the blog post is not found
            return NotFound(new ResponseModel<bool>
            {
                Message = "Blog post not found",
                Result = false,
                Status = "Not Found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0
            });
        }

        // Update the blog post to reflect a soft delete
        blogModel.IsActive = false;
        blogModel.DeletedDate = DateTime.UtcNow;
        blogModel.DeletedById = user.Id;

        // Save the updated blog post to the database
        _DBContext.Blogs.Update(blogModel);
        _DBContext.SaveChanges();

        // Return a response indicating the deletion was successful
        return Ok(new ResponseModel<object>
        {
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Message = "Successfully deleted",
            Result = true
        });
    }

}