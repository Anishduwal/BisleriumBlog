using System.Net;

namespace BisleriumBlog.Application.Features.Response.Base;

public class ResponseModel<T>
{
    public string Status { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
    public int? TotalCount { get; set; }
}
