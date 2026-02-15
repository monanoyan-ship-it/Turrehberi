using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class BlogPost : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; } = BlogCategories.Ids.TravelTips;
    public int StatusId { get; set; } = BlogStatuses.Ids.Draft;
    public int AuthorId { get; set; }
    public int? CompanyId { get; set; }
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }

    // Navigation
    public virtual Visitor Author { get; set; } = null!;
    public virtual Company? Company { get; set; }
    public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
}
