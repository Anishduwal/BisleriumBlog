using Azure;
using BisleriumBlog.Application.Common;
using BisleriumBlog.Application.Features.Response.Base;
using BisleriumBlog.Application.Features.Request.Account;
using BisleriumBlog.Application.Features.Response.Account;
using BisleriumBlog.Application.Models;
using BisleriumBlog.Application.Utilities;
using BisleriumBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BisleriumBlog.Api.Controllers;

[ApiController]
[Route("api/Account")]
public class AccountController : Controller
{
    private readonly JWTSettings _jwtSettings;
    private readonly ApplicationDbContext _DBContext;

    public AccountController(IOptions<JWTSettings> jwtSettings, ApplicationDbContext dbContext)
    {
        _jwtSettings = jwtSettings.Value;
        _DBContext = dbContext;

    }

    [HttpPost("sign-in")]
    public IActionResult LoginUser(LoginRequest loginRequest)
    {
        // Fetch the user from the database
        var user = _DBContext.Users.SingleOrDefault(x => x.EmailAddress == loginRequest.EmailID);

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

        // Validate the provided password
        var isPasswordValid = PasswordManager.ValidatePassword(loginRequest.Password, user.Password);

        if (!isPasswordValid)
        {
            // Return a 401 Unauthorized response if the password is incorrect
            return Unauthorized(new ResponseModel<bool>
            {
                Message = "Invalid password",
                Result = false,
                Status = "Unauthorized",
                StatusCode = HttpStatusCode.Unauthorized,
                TotalCount = 0
            });
        }

        // Fetch the user's role
        var role = _DBContext.Roles.SingleOrDefault(r => r.Id == user.RoleId);

        // Prepare JWT claims
        var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.EmailAddress),
        new Claim(ClaimTypes.Role, role?.Name ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // Create JWT token
        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(30);

        var jwtToken = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims: authClaims,
            signingCredentials: credentials,
            expires: expiration
        );

        // Prepare user details response
        var userDetails = new UserResponse
        {
            Id = user.Id,
            Name = user.FullName,
            UserName = user.UserName,
            EmailID = user.EmailAddress,
            RoleId = role?.Id ?? 0,
            Role = role?.Name ?? string.Empty,
            ImagePath = string.IsNullOrEmpty(user.ImagePath) ? "Sample.svg" : user.ImagePath,
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken)
        };

        // Return successful response with user details and token
        return Ok(new ResponseModel<UserResponse>
        {
            Message = "Successfully authenticated",
            Result = userDetails,
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1
        });
    }

    [HttpPost("create-user")]
    public IActionResult RegisterUser(RegisterRequest register)
    {
        // Check if a user with the same email or username already exists
        var existingUser = _DBContext.Users
            .SingleOrDefault(x => x.EmailAddress == register.EmailID || x.UserName == register.UserName);

        if (existingUser != null)
        {
            // Return a 400 Bad Request response if the user already exists
            return BadRequest(new ResponseModel<bool>
            {
                Message = "User with the same email address or username already exists",
                Result = false,
                Status = "Bad Request",
                StatusCode = HttpStatusCode.BadRequest,
                TotalCount = 0
            });
        }

        // Fetch the role for the new user
        var role = _DBContext.Roles.SingleOrDefault(r => r.Name == Common.Roles.Blogger);

        if (role == null)
        {
            // Return a 500 Internal Server Error if the role does not exist
            return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseModel<bool>
            {
                Message = "Role not found",
                Result = false,
                Status = "Error",
                StatusCode = HttpStatusCode.InternalServerError,
                TotalCount = 0
            });
        }

        // Create and add the new user
        var appUser = new User
        {
            FullName = register.FullName,
            EmailAddress = register.EmailID,
            RoleId = role.Id,
            Password = PasswordManager.GenerateHash(register.Password),
            UserName = register.UserName,
            ContactNo = register.MobileNumber,
            ImagePath = register.ImagePath
        };

        _DBContext.Users.Add(appUser);
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Successfully registered",
            Result = true,
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1
        });
    }

}
