namespace BisleriumBlog.Application.Common;

public class Common
{
    public class Roles
    {
        public const string Admin = "AdminUser";
        public const string Blogger = "BloggerUser";
    }

    public class Passwords
    {
        public const string AdminPassword = "Admin@101";
        public const string BloggerPassword = "Blogger@101";
    }

    public class FilePath
    {
        public static string UserImagesFilePath => @"UserImages\";

        public static string BlogImagesFilePath => @"BlogImages\";
    }
}
