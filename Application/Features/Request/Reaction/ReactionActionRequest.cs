namespace BisleriumBlog.Application.Features.Request.Reaction;

public class ReactionActionRequest
{
    public int? BlogID { get; set; }

    public int? ReactionID { get; set; }

    public string? Comment { get; set; }

    public int? CommentID { get; set; }
}