using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Core.Entities;

public class Visitor : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Kullanici tipi ID - UserTypes.Ids uzerinden erisim
    /// 0: Visitor, 1: CompanyOwner, 2: Staff, 3: Admin
    /// </summary>
    public int UserTypeId { get; set; } = UserTypes.Ids.Visitor;

    /// <summary>
    /// Kullanicinin tercih ettigi dil kodu (tr, en, de, vb.)
    /// </summary>
    public string PreferredLanguage { get; set; } = "tr";

    /// <summary>
    /// Kullanicinin adresi
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Kullanicinin dogum tarihi
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Profil resmi URL'i
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Sifre sifirlama token'i
    /// </summary>
    public string? PasswordResetToken { get; set; }

    /// <summary>
    /// Sifre sifirlama token'inin gecerlilik suresi
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }

    /// <summary>
    /// Email adresi dogrulandi mi?
    /// </summary>
    public bool EmailVerified { get; set; } = false;

    /// <summary>
    /// Email dogrulama token'i
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// Email dogrulama token'inin gecerlilik suresi
    /// </summary>
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    // Firma sahibi ise iliskili firma
    public int? CompanyId { get; set; }
    public virtual Company? Company { get; set; }

    // Iliskiler
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<TourReview> Reviews { get; set; } = new List<TourReview>();
    public virtual ICollection<ReviewHelpful> HelpfulVotes { get; set; } = new List<ReviewHelpful>();
    public virtual ICollection<ReviewReply> ReviewReplies { get; set; } = new List<ReviewReply>();
    public virtual ICollection<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
}
