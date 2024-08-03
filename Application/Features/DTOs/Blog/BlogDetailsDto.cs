namespace BisleriumBlog.Application.Features.DTOs.Blog;

public class BlogDetailsDto
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string Locations { get; set; }

    public string Reactions { get; set; }

    public List<string> Images { get; set; }
}
