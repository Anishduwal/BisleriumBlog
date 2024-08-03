using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BisleriumBlog.Application.Models;

namespace BisleriumBlog.Domain.Base
{
    public class CommonEntity<TPrimaryKey>
    {
        [Key]
        public int Id { get; set; } = default!;

        public int CreatedById { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [ForeignKey("CreatedBy")]
        public virtual User? CreatedUser { get; set; }
        public int UpdatedById { get; set; } = new();

        public int? LastUpdatedById { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedUser { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public int? DeletedById { get; set; }

        public DateTime? DeletedDate { get; set; }

        [ForeignKey("DeletedBy")]
        public virtual User? DeletedUser { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
