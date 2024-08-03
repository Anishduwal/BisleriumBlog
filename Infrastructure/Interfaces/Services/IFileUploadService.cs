namespace BisleriumBlog.Infrastructure.Interfaces.Services;

public interface IFileUploadService
{
    string SaveUploadedFile(string uploadedFilePath, IFormFile file);
}
