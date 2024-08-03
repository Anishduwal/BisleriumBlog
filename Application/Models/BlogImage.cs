using BisleriumBlog.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBlog.Application.Models;

public class BlogImage : CommonEntity<int>
{
    public string ImagePath { get; set; }

    public int BlogId { get; set; }

    [ForeignKey("BlogId")]
    public virtual Blog Blog { get; set; }
}
