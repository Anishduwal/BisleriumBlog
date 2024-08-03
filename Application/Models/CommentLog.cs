using BisleriumBlog.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBlog.Application.Models;

public class CommentLog : CommonEntity<int>
{
    public int CommentId { get; set; }

    public string Message { get; set; }

    [ForeignKey("CommentId")]
    public virtual Comment? Comment { get; set; }
}
