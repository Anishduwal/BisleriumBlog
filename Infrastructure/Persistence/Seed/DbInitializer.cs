using BisleriumBlog.Application.Models;
using BisleriumBlog.Application.Utilities;
using BisleriumBlog.Infrastructure.Interfaces.Repositories.Base;

namespace BisleriumBlog.Infrastructure.Persistence.Seed;

public class DbInitializer(IGenericRepo genericRepository) : IDbInitializer
{
    public void Initialize()
    {
        try
        {
            if (!genericRepository.GetData<Role>().Any())
            {
                var admin = new Role()
                {
                    Name = "Admin",
                    Description = ""
                };
                var blogger = new Role()
                {
                    Name = "Blogger",
                    Description = ""
                };

                genericRepository.Add(admin);
                genericRepository.Add(blogger);
            }

            if (genericRepository.GetData<User>().Any()) return;

            var adminRole = genericRepository.GetFirstOrDefault<Role>(x => x.Name == "Admin");

            var superAdminUser = new User
            {
                FullName = "Super Admin",
                EmailAddress = "superadmin@superadmin.com",
                UserName = "superadmin@superadmin.com",
                Password = Password.HashSecret("Admin@123"),
                RoleId = adminRole.Id,
                ContactNo = "+977 9803364638",
                ImageURL = null
            };

            genericRepository.Add(superAdminUser);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}