namespace ErkanTatilPlani.Core.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
