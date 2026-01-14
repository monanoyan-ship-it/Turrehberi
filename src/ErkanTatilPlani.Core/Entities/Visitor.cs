namespace ErkanTatilPlani.Core.Entities;

public class Visitor : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
