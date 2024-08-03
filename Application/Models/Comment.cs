using BisleriumBlog.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBlog.Application.Models;

public class Comment : CommonEntity<int>
{
    public string Message { get; set; }

    public bool IsCommentForBlog { get; set; }

    public bool IsCommentForComment { get; set; }

    public int? BlogId { get; set; }

    public int? CommentId { get; set; }

    [ForeignKey("BlogId")]
    public virtual Blog? Blog { get; set; }

    [ForeignKey("CommentId")]
    public virtual Comment? Comments { get; set; }
}
