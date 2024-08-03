using BisleriumBlog.Application.Models;
using BisleriumBlog.Application.Utilities;
using System;

namespace BisleriumBlog.Infrastructure.Persistence.Seed;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseInitializer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Initialize()
    {
        try
        {
            // Seed Roles if they don't exist
            if (!_dbContext.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "AdminUser", Description = "Administrator with full access" },
                    new Role { Name = "BloggerUser", Description = "User with blogging rights" }
                };

                _dbContext.Roles.AddRange(roles);
                _dbContext.SaveChanges();
            }

            // Seed Users if they don't exist
            if (!_dbContext.Users.Any())
            {
                var adminRole = _dbContext.Roles
                    .FirstOrDefault(r => r.Name == "AdminUser");

                if (adminRole != null)
                {
                    var superAdmin = new User
                    {
                        FullName = "Super Admin",
                        EmailAddress = "admin@islingtoncollege.com",
                        UserName = "admin@islingtoncollege.com",
                        Password = PasswordManager.GenerateHash("Admin@101"),
                        RoleId = adminRole.Id,
                        ContactNo = "+977 9823475404",
                        ImagePath = null
                    };

                    _dbContext.Users.Add(superAdmin);
                    _dbContext.SaveChanges();
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception (consider using a proper logging framework)
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }
}
