namespace BisleriumBlog.Application.Features.Request.Upload;

public class UploadRequest
{
    public List<IFormFile> Files { get; set; }

    public string FilePath { get; set; }
}
