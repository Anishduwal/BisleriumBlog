namespace BisleriumBlog.Application.Features.Response.Home;

public class BlogPostDetailsResponse : UserActivity
{
    public int BlogId { get; set; }

    public string Body { get; set; }

    public string Title { get; set; }

    public string BloggerName { get; set; }

    public string BloggerImage { get; set; }

    public string Address { get; set; }

    public string Reaction { get; set; }

    public int CommentCount { get; set; }

    public string UploadedDuration { get; set; }

    public bool IsUpdated { get; set; }

    public int PopularityPoints { get; set; }

    public DateTime CreatedDate { get; set; }

    public List<string> Images { get; set; }

    public List<PostComments> Comments { get; set; }
}

public class PostComments : UserActivity
{
    public int CommentId { get; set; }

    public string ImagePath { get; set; }

    public string CommentedDuration { get; set; }

    public bool IsUpdated { get; set; }

    public string CommentedBy { get; set; }

    public string Comment { get; set; }

    public List<PostComments> Comments { get; set; }
}

public class UserActivity
{
    public int UpVotes { get; set; }

    public bool IsUpVotedByUser { get; set; }

    public int DownVotes { get; set; }

    public bool IsDownVotedByUser { get; set; }
}
