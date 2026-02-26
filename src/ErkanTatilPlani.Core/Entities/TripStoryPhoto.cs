namespace ErkanTatilPlani.Core.Entities;

public class TripStoryPhoto : BaseEntity
{
    public int TripStoryId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }

    public virtual TripStory TripStory { get; set; } = null!;
}
