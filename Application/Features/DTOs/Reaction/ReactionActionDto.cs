namespace BisleriumBlog.Application.Features.DTOs.Reaction;

public class ReactionActionDto
{
    public int? BlogId { get; set; }

    public int? CommentId { get; set; }

    public int? ReactionId { get; set; }

    public string? Comment { get; set; }
}