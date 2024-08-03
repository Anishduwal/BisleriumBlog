namespace BisleriumBlog.Application.Features.Request.Blog;

public class BlogCreateRequest
{
    public string BlogTitle { get; set; }

    public string Body { get; set; }

    public string Address { get; set; }

    public List<string>? Images { get; set; }

    public string Reactions { get; set; }

}
