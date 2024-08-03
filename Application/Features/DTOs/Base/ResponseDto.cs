using System.Net;

namespace BisleriumBlog.Application.Features.DTOs.Base;

public class ResponseDto<T>
{
    public HttpStatusCode StatusCode { get; set; }

    public string Status { get; set; }

    public string Message { get; set; }

    public int? TotalCount { get; set; }

    public T Result { get; set; }
}
