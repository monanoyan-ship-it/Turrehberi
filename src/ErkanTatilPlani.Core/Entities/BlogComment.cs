namespace ErkanTatilPlani.Core.Entities;

public class BlogComment : BaseEntity
{
    public int BlogPostId { get; set; }
    public int VisitorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }

    // Navigation
    public virtual BlogPost BlogPost { get; set; } = null!;
    public virtual Visitor Visitor { get; set; } = null!;
    public virtual BlogComment? ParentComment { get; set; }
    public virtual ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
}
