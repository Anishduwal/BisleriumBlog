using BisleriumBlog.Application.Utilities;
using BisleriumBlog.Infrastructure.Interfaces.Services;

namespace BisleriumBlog.Infrastructure.Implementations.Services;

public class FileUploadService(IWebHostEnvironment webHostEnvironment) : IFileUploadService
{
    public string SaveUploadedFile(string targetPath, IFormFile file)
    {
        var fullPath = Path.Combine(webHostEnvironment.WebRootPath, targetPath);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        var savedFileName = SaveFileToDirectory(fullPath, file);

        return savedFileName;
    }

    private static string SaveFileToDirectory(string directoryPath, IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName);

        var uniqueFileName = GenerateUniqueFileName(fileExtension);

        using var fileStream = new FileStream(Path.Combine(directoryPath, uniqueFileName), FileMode.Create);
        file.CopyTo(fileStream);

        return uniqueFileName;
    }

    //Generate random name from Guid
    private static string GenerateUniqueFileName(string fileExtension)
    {
        return $"{Guid.NewGuid()}{fileExtension}";
    }

}