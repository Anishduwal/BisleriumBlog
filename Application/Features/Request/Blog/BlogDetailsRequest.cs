namespace BisleriumBlog.Application.Features.Request.Blog;

public class BlogDetailsRequest
{
    public int Id { get; set; }

    public string BlogTitle { get; set; }

    public string Body { get; set; }

    public string Address { get; set; }

    public string Reactions { get; set; }

    public List<string> Images { get; set; }
}
