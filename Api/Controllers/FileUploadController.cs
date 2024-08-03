using BisleriumBlog.Application.Common;
using BisleriumBlog.Application.Features.Response.Base;
using BisleriumBlog.Application.Features.Request.Upload;
using BisleriumBlog.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;

namespace BisleriumBlog.Api.Controllers;

[ApiController]
[Route("api/file-upload")]
public class FileUploadController : Controller
{
    private readonly IFileUploadService _fileUploadService;

    public FileUploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    /*This method handles file uploads, validates file sizes, determines the file path based on provided index, 
    and returns a success response with the list of uploaded file names*/

    [HttpPost]
    public IActionResult UploadFile([FromForm] UploadRequest uploads)
    {
        // Attempt to parse the file path index from the request
        if (!int.TryParse(uploads.FilePath, out int filePathIndex))
        {
            // Return a 400 Bad Request if the file path index is invalid
            return BadRequest(new ResponseModel<object>
            {
                Message = "Invalid file path index.",
                StatusCode = HttpStatusCode.BadRequest,
                TotalCount = 0,
                Status = "Bad Request",
                Result = false
            });
        }

        // Determine the file path based on the provided index
        var filePaths = filePathIndex switch
        {
            1 => Common.FilePath.UserImagesFilePath,
            2 => Common.FilePath.BlogImagesFilePath,
            _ => string.Empty
        };

        // Return a 400 Bad Request if the file path could not be determined
        if (string.IsNullOrEmpty(filePaths))
        {
            return BadRequest(new ResponseModel<object>
            {
                Message = "File path is not recognized.",
                StatusCode = HttpStatusCode.BadRequest,
                TotalCount = 0,
                Status = "Bad Request",
                Result = false
            });
        }

        // Define a constant for the maximum allowed file size (3 MB)
        const long MaxFileSize = 3 * 1024 * 1024;

        // Check if any of the uploaded files exceed the maximum allowed size
        if (uploads.Files.Any(file => file.Length > MaxFileSize))
        {
            // Return a 400 Bad Request if any file exceeds the size limit
            return BadRequest(new ResponseModel<object>
            {
                Message = "File size must not exceed 3 MB.",
                StatusCode = HttpStatusCode.BadRequest,
                TotalCount = 0,
                Status = "Bad Request",
                Result = false
            });
        }

        // Process and save each uploaded file, collecting their names
        var fileNames = uploads.Files.Select(file => _fileUploadService.SaveUploadedFile(filePaths, file)).ToList();

        // Construct a response model with the list of saved file names
        var response = new ResponseModel<List<string>>
        {
            Message = "Files uploaded successfully.",
            Result = fileNames,
            StatusCode = HttpStatusCode.OK,
            Status = "Success",
            TotalCount = fileNames.Count
        };

        // Return the response indicating successful file upload
        return Ok(response);
    }

}
