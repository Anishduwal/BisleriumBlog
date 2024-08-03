using BisleriumBlog.Application.Models;
using BisleriumBlog.Application.Utilities;
using BisleriumBlog.Application.Features.Response.Base;
using BisleriumBlog.Application.Features.Request.Email;
using BisleriumBlog.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BisleriumBlog.Application.Features.Request.Profile;
using BisleriumBlog.Application.Features.Response.Profile;
using BisleriumBlog.Infrastructure.Persistence;

namespace BisleriumBlog.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly IMailService _emailService;
    private readonly ApplicationDbContext _DBContext;

    public ProfileController(IMailService emailService, ApplicationDbContext dbContext, IUserService userService)
    {
        _emailService = emailService;
        _DBContext = dbContext;
        _userService = userService;
    }

    [HttpGet("profile-details")]
    public IActionResult ProfileDetails()
    {
        // Retrieve the user ID from the user service
        var userId = _userService.UserId;

        // Fetch the user from the database context
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            // If the user does not exist, return a not found response
            return NotFound(new ResponseModel<ProfileDetailsResponse>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = null
            });
        }

        // Fetch the user's role from the database context
        var role = _DBContext.Roles.Find(user.RoleId);

        if (role == null)
        {
            // If the role does not exist, return a not found response
            return NotFound(new ResponseModel<ProfileDetailsResponse>
            {
                Message = "Role not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = null
            });
        }

        // Construct the profile details response
        var result = new ProfileDetailsResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
            EmailID = user.EmailAddress,
            RoleId = role.Id,
            RoleName = role.Name,
            ImagePath = string.IsNullOrEmpty(user.ImagePath) ? "Sample.svg" : user.ImagePath, // Use default image if ImageURL is null or empty
            ContactNumber = user.ContactNo ?? string.Empty
        };

        // Return a success response
        return Ok(new ResponseModel<ProfileDetailsResponse>
        {
            Message = "Successfully Fetched",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Status = "Success",
            Result = result
        });
    }
    [HttpPatch("update-profile")]
    public IActionResult UpdateProfileDetails(ProfileDetailsResponse profileDetails)
    {
        // Retrieve the user by ID from the database context
        var user = _DBContext.Users.Find(profileDetails.UserId);

        if (user == null)
        {
            // If the user does not exist, return a not found response
            return NotFound(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = false
            });
        }

        // Update user details
        user.FullName = profileDetails.FullName;
        user.ContactNo = profileDetails.ContactNumber;
        user.EmailAddress = profileDetails.EmailID;

        // Mark the user entity as modified
        _DBContext.Users.Update(user);

        // Save changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Successfully Updated",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Status = "Success",
            Result = true
        });
    }

    [HttpDelete("remove-user-profile")]
    public IActionResult RemoveUserProfile()
    {
        // Get the ID of the currently logged-in user
        var currentUserId = _userService.UserId;

        // Retrieve the user record from the database
        var user = _DBContext.Users.Find(currentUserId);

        if (user == null)
        {
            // Return a not found response if the user does not exist
            return NotFound(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = false
            });
        }

        // Retrieve all blogs created by the user
        var userBlogs = _DBContext.Blogs.Where(b => b.CreatedById == user.Id).ToList();

        // Retrieve all blog images associated with the user's blogs
        var associatedBlogImages = _DBContext.BlogImages.Where(bi => userBlogs.Select(b => b.Id).Contains(bi.BlogId)).ToList();

        // Retrieve all comments made by the user
        var userComments = _DBContext.Comments.Where(c => c.CreatedById == user.Id).ToList();

        // Retrieve all reactions made by the user
        var userReactions = _DBContext.Reactions.Where(r => r.CreatedById == user.Id).ToList();

        // Remove all reactions
        _DBContext.Reactions.RemoveRange(userReactions);

        // Remove all comments
        _DBContext.Comments.RemoveRange(userComments);

        // Remove all blog images
        _DBContext.BlogImages.RemoveRange(associatedBlogImages);

        // Remove all blogs
        _DBContext.Blogs.RemoveRange(userBlogs);

        // Finally, remove the user
        _DBContext.Users.Remove(user);

        // Save all changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "User profile and associated data successfully deleted",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Status = "Success",
            Result = true
        });
    }

    [HttpPost("update-password")]
    public IActionResult UpdatePassword(ChangePasswordRequest passwordUpdateRequest)
    {
        // Obtain the current user ID from the user service
        var currentUserId = _userService.UserId;

        // Retrieve the user record from the database
        var user = _DBContext.Users.SingleOrDefault(u => u.Id == currentUserId);

        if (user == null)
        {
            // Return an error response if the user does not exist
            return NotFound(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = false
            });
        }

        // Check if the provided current password matches the stored password hash
        bool isPasswordValid = PasswordManager.ValidatePassword(passwordUpdateRequest.CurrentPassword, user.Password);

        if (isPasswordValid)
        {
            // Update the user's password if the current password is correct
            user.Password = PasswordManager.GenerateHash(passwordUpdateRequest.NewPassword);

            // Mark the user entity as modified
            _DBContext.Users.Update(user);

            // Persist changes to the database
            _DBContext.SaveChanges();

            // Return a success response indicating the password was updated
            return Ok(new ResponseModel<object>
            {
                Message = "Password updated successfully",
                StatusCode = HttpStatusCode.OK,
                TotalCount = 1,
                Status = "Success",
                Result = true
            });
        }

        // Return an error response if the current password is incorrect
        return BadRequest(new ResponseModel<object>
        {
            Message = "Current password is incorrect",
            StatusCode = HttpStatusCode.BadRequest,
            TotalCount = 1,
            Status = "Invalid",
            Result = false
        });
    }

    [HttpPost("request-password-reset")]
    public IActionResult RequestPasswordReset(string userEmail)
    {
        // Find the user by their email address
        var user = _DBContext.Users.SingleOrDefault(x => x.EmailAddress == userEmail);

        if (user == null)
        {
            // If no user is found with the provided email, return an error response
            return BadRequest(new ResponseModel<object>
            {
                Message = "No account found with the provided email address.",
                StatusCode = HttpStatusCode.BadRequest,
                TotalCount = 0,
                Status = "Error",
                Result = false
            });
        }

        // Define a temporary new password
        const string temporaryPassword = "Blogger@101";

        // Compose the email message
        var emailBody =
            $"Hello {user.FullName},<br><br>" +
            $"Your password has been reset successfully. Your new temporary password is {temporaryPassword}.<br><br>" +
            $"Please use this password to log in and change it to something more secure.<br><br>" +
            $"Best Regards,<br>" +
            $"The BisleriumBlog Team.";

        // Create the email request
        var emailRequest = new EmailRequest
        {
            EmailID = user.EmailAddress,
            Message = emailBody,
            Subject = "Password Reset - BisleriumBlog"
        };

        // Send the password reset email
        _emailService.SendMail(emailRequest);

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Password reset email sent successfully.",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1,
            Status = "Success",
            Result = true
        });
    }

}