namespace ErkanTatilPlani.Core.Entities;

public class Tour : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int MaxCapacity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; } = false;

    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
