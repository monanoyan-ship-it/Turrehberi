using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Services;
using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ICacheService _cache;
    private readonly IFileUploadService _fileUploadService;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public CompaniesController(AppDbContext context, IWebHostEnvironment env, ICacheService cache, IFileUploadService fileUploadService)
    {
        _context = context;
        _env = env;
        _cache = cache;
        _fileUploadService = fileUploadService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        return await _context.Companies.Where(c => c.IsActive).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();
        return company;
    }

    [HttpGet("{id}/tours")]
    public async Task<ActionResult<IEnumerable<Tour>>> GetCompanyTours(int id)
    {
        var tours = await _context.Tours
            .Where(t => t.CompanyId == id && t.IsActive)
            .ToListAsync();
        return Ok(tours);
    }

    // ===============================================
    // PUBLIC FIRMA PROFILI (SEO-FRIENDLY)
    // ===============================================

    /// <summary>
    /// Public firma profili - slug ile erisim (SEO icin)
    /// </summary>
    [HttpGet("profile/{slug}")]
    public async Task<ActionResult<object>> GetCompanyProfile(string slug)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && c.StatusId == CompanyStatuses.Ids.Approved);

        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        // Firmanin turlari
        var tours = await _context.Tours
            .Where(t => t.CompanyId == company.Id && t.IsActive)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Destination,
                t.Price,
                t.DurationDays,
                t.MaxCapacity,
                t.ImageUrl,
                t.IsFeatured,
                t.AverageRating,
                t.ReviewCount
            })
            .ToListAsync();

        // Firmanin toplam yorum istatistikleri
        var reviewStats = await _context.TourReviews
            .Where(r => r.Tour.CompanyId == company.Id && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .GroupBy(r => 1)
            .Select(g => new
            {
                TotalReviews = g.Count(),
                AverageRating = g.Average(r => r.OverallRating),
                FiveStarCount = g.Count(r => r.OverallRating == 5),
                FourStarCount = g.Count(r => r.OverallRating == 4),
                ThreeStarCount = g.Count(r => r.OverallRating == 3),
                TwoStarCount = g.Count(r => r.OverallRating == 2),
                OneStarCount = g.Count(r => r.OverallRating == 1)
            })
            .FirstOrDefaultAsync();

        // Son 5 yorum
        var recentReviews = await _context.TourReviews
            .Include(r => r.Visitor)
            .Include(r => r.Tour)
            .Where(r => r.Tour.CompanyId == company.Id && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                r.OverallRating,
                r.Title,
                r.Comment,
                r.Pros,
                r.Cons,
                r.IsVerified,
                r.WouldRecommend,
                r.CreatedAt,
                TourName = r.Tour.Name,
                VisitorName = r.Visitor.FirstName + " " + (r.Visitor.LastName.Length > 0 ? r.Visitor.LastName[0] + "." : "")
            })
            .ToListAsync();

        return Ok(new
        {
            // Firma bilgileri
            company = new
            {
                company.Id,
                company.Name,
                company.Slug,
                company.Description,
                company.Tagline,
                company.Email,
                company.Phone,
                company.Address,
                company.City,
                company.Website,
                company.LogoUrl,
                company.CoverImageUrl,
                company.FoundedYear,
                company.SocialLinks,
                company.MetaTitle,
                company.MetaDescription
            },
            // Turlar
            tours,
            tourCount = tours.Count,
            // Yorum istatistikleri
            stats = reviewStats != null ? new
            {
                totalReviews = reviewStats.TotalReviews,
                averageRating = Math.Round(reviewStats.AverageRating, 1),
                ratingDistribution = new
                {
                    fiveStar = reviewStats.FiveStarCount,
                    fourStar = reviewStats.FourStarCount,
                    threeStar = reviewStats.ThreeStarCount,
                    twoStar = reviewStats.TwoStarCount,
                    oneStar = reviewStats.OneStarCount
                }
            } : new { totalReviews = 0, averageRating = 0.0, ratingDistribution = new { fiveStar = 0, fourStar = 0, threeStar = 0, twoStar = 0, oneStar = 0 } },
            // Son yorumlar
            recentReviews
        });
    }

    /// <summary>
    /// Public firma listesi (sadece onaylanmis firmalar)
    /// </summary>
    [HttpGet("public")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "city", "search", "sort" })] // 5 dakika cache
    public async Task<ActionResult<object>> GetPublicCompanies(
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null)
    {
        var query = _context.Companies
            .Where(c => c.IsActive && c.StatusId == CompanyStatuses.Ids.Approved);

        // Sehir filtresi
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(c => c.City.ToLower().Contains(city.ToLower()));
        }

        // Arama
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.Description.ToLower().Contains(searchLower) ||
                c.City.ToLower().Contains(searchLower) ||
                c.Tagline.ToLower().Contains(searchLower));
        }

        // Her firma icin rating bilgisi hesapla
        var companiesWithRating = await query
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Tagline,
                c.City,
                c.LogoUrl,
                c.CoverImageUrl,
                c.FoundedYear,
                TourCount = c.Tours.Count(t => t.IsActive),
                ReviewCount = _context.TourReviews
                    .Where(r => r.Tour.CompanyId == c.Id && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
                    .Count(),
                AverageRating = _context.TourReviews
                    .Where(r => r.Tour.CompanyId == c.Id && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
                    .Any()
                    ? Math.Round(_context.TourReviews
                        .Where(r => r.Tour.CompanyId == c.Id && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
                        .Average(r => r.OverallRating), 1)
                    : 0.0
            })
            .ToListAsync();

        // Siralama
        var sortedCompanies = sort switch
        {
            "name_asc" => companiesWithRating.OrderBy(c => c.Name).ToList(),
            "name_desc" => companiesWithRating.OrderByDescending(c => c.Name).ToList(),
            "rating" => companiesWithRating.OrderByDescending(c => c.AverageRating).ThenByDescending(c => c.ReviewCount).ToList(),
            "tours" => companiesWithRating.OrderByDescending(c => c.TourCount).ToList(),
            "newest" => companiesWithRating.OrderByDescending(c => c.Id).ToList(),
            _ => companiesWithRating.OrderBy(c => c.Name).ToList()
        };

        // Sehir listesi (filtreleme icin)
        var cities = await _context.Companies
            .Where(c => c.IsActive && c.StatusId == CompanyStatuses.Ids.Approved && !string.IsNullOrEmpty(c.City))
            .Select(c => c.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(new { companies = sortedCompanies, cities, total = sortedCompanies.Count });
    }

    [HttpPost]
    public async Task<ActionResult<Company>> CreateCompany(Company company)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, Company company)
    {
        if (id != company.Id) return BadRequest();
        company.UpdatedAt = DateTime.UtcNow;
        _context.Entry(company).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();
        company.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ===============================================
    // FIRMA ONAY IS AKISI
    // ===============================================

    /// <summary>
    /// Onay bekleyen firmalari listele
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<Company>>> GetPendingCompanies()
    {
        return await _context.Companies
            .Where(c => c.IsActive && c.StatusId == CompanyStatuses.Ids.Pending)
            .OrderBy(c => c.ApplicationDate)
            .ToListAsync();
    }

    /// <summary>
    /// Firma onayla
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveCompany(int id, [FromBody] ApproveCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();

        if (company.StatusId != CompanyStatuses.Ids.Pending)
            return BadRequest(new { message = "Sadece bekleyen basvurular onaylanabilir" });

        company.StatusId = CompanyStatuses.Ids.Approved;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = request.ReviewedById;
        company.ReviewNotes = request.ReviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Firma onaylandi", company });
    }

    /// <summary>
    /// Firma reddet
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectCompany(int id, [FromBody] RejectCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();

        if (company.StatusId != CompanyStatuses.Ids.Pending)
            return BadRequest(new { message = "Sadece bekleyen basvurular reddedilebilir" });

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            return BadRequest(new { message = "Red sebebi zorunludur" });

        company.StatusId = CompanyStatuses.Ids.Rejected;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = request.ReviewedById;
        company.RejectionReason = request.RejectionReason;
        company.ReviewNotes = request.ReviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Firma basvurusu reddedildi", company });
    }

    /// <summary>
    /// Firma askiya al
    /// </summary>
    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendCompany(int id, [FromBody] SuspendCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();

        if (company.StatusId != CompanyStatuses.Ids.Approved)
            return BadRequest(new { message = "Sadece onaylanmis firmalar askiya alinabilir" });

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Askiya alma sebebi zorunludur" });

        company.StatusId = CompanyStatuses.Ids.Suspended;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = request.ReviewedById;
        company.ReviewNotes = request.Reason;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Firma askiya alindi", company });
    }

    // ===============================================
    // FIRMA DASHBOARD
    // ===============================================

    /// <summary>
    /// Firma sahibi icin dashboard istatistikleri
    /// </summary>
    [HttpGet("{id}/dashboard")]
    public async Task<ActionResult<object>> GetCompanyDashboard(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        // Firmanin turlari
        var companyTours = await _context.Tours
            .Where(t => t.CompanyId == id && t.IsActive)
            .ToListAsync();

        var tourIds = companyTours.Select(t => t.Id).ToList();

        // Istatistikler
        var totalTours = companyTours.Count;
        var activeTours = companyTours.Count(t => t.IsActive);

        // Rezervasyon istatistikleri
        var reservations = await _context.Reservations
            .Where(r => tourIds.Contains(r.TourId) && r.IsActive)
            .ToListAsync();

        var totalReservations = reservations.Count;
        var pendingReservations = reservations.Count(r => r.Status == ReservationStatus.Pending);
        var confirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed);
        var completedReservations = reservations.Count(r => r.Status == ReservationStatus.Completed);

        // Gelir hesaplamasi (onaylanmis ve tamamlanmis rezervasyonlar)
        var totalRevenue = reservations
            .Where(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed)
            .Sum(r => r.TotalPrice);

        // Bu ayin geliri
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthlyRevenue = reservations
            .Where(r => (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed)
                && r.CreatedAt >= startOfMonth)
            .Sum(r => r.TotalPrice);

        // Yorum istatistikleri
        var reviews = await _context.TourReviews
            .Where(r => tourIds.Contains(r.TourId) && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .ToListAsync();

        var totalReviews = reviews.Count;
        var averageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.OverallRating), 1) : 0;

        // Son 5 rezervasyon
        var recentReservations = await _context.Reservations
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .Where(r => tourIds.Contains(r.TourId) && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                CustomerName = r.Visitor.FirstName + " " + r.Visitor.LastName,
                r.StartDate,
                r.NumberOfPeople,
                r.TotalPrice,
                Status = r.Status.ToString(),
                r.CreatedAt
            })
            .ToListAsync();

        // Son 5 yorum
        var recentReviews = await _context.TourReviews
            .Include(r => r.Tour)
            .Include(r => r.Visitor)
            .Where(r => tourIds.Contains(r.TourId) && r.IsActive && r.StatusId == ReviewStatuses.Ids.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                TourName = r.Tour.Name,
                VisitorName = r.Visitor.FirstName + " " + (r.Visitor.LastName.Length > 0 ? r.Visitor.LastName[0] + "." : ""),
                r.OverallRating,
                r.Title,
                Comment = r.Comment.Length > 100 ? r.Comment.Substring(0, 100) + "..." : r.Comment,
                r.CreatedAt
            })
            .ToListAsync();

        // Tur performansi
        var tourPerformance = new List<object>();
        foreach (var tour in companyTours)
        {
            var tourReservations = reservations.Where(r => r.TourId == tour.Id).ToList();
            var tourReviews = reviews.Where(r => r.TourId == tour.Id).ToList();

            tourPerformance.Add(new
            {
                tour.Id,
                tour.Name,
                ReservationCount = tourReservations.Count,
                Revenue = tourReservations
                    .Where(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed)
                    .Sum(r => r.TotalPrice),
                AverageRating = tourReviews.Any() ? Math.Round(tourReviews.Average(r => r.OverallRating), 1) : 0,
                ReviewCount = tourReviews.Count
            });
        }

        return Ok(new
        {
            stats = new
            {
                totalTours,
                activeTours,
                totalReservations,
                pendingReservations,
                confirmedReservations,
                completedReservations,
                totalRevenue,
                monthlyRevenue,
                totalReviews,
                averageRating
            },
            recentReservations,
            recentReviews,
            tourPerformance,
            settings = new
            {
                depositPercentage = company.DepositPercentage
            }
        });
    }

    /// <summary>
    /// Firma ayarlarini guncelle
    /// </summary>
    [HttpPut("{id}/settings")]
    public async Task<IActionResult> UpdateCompanySettings(int id, [FromBody] UpdateCompanySettingsRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        // Validasyon
        if (request.DepositPercentage < 10 || request.DepositPercentage > 100)
            return BadRequest(new { message = "On odeme yuzdesi 10 ile 100 arasinda olmalidir" });

        company.DepositPercentage = request.DepositPercentage;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Ayarlar guncellendi", depositPercentage = company.DepositPercentage });
    }

    /// <summary>
    /// Askiya alinan firmayi tekrar aktifle
    /// </summary>
    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> ReactivateCompany(int id, [FromBody] ReactivateCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return NotFound();

        if (company.StatusId != CompanyStatuses.Ids.Suspended)
            return BadRequest(new { message = "Sadece askiya alinmis firmalar tekrar aktiflenebilir" });

        company.StatusId = CompanyStatuses.Ids.Approved;
        company.ReviewedAt = DateTime.UtcNow;
        company.ReviewedById = request.ReviewedById;
        company.ReviewNotes = request.ReviewNotes ?? string.Empty;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Firma tekrar aktiflendi", company });
    }

    // ===============================================
    // SOZLESME YUKLEME ISLEMLERI
    // ===============================================

    /// <summary>
    /// Firma sozlesmesi yukle (PDF, max 10MB)
    /// </summary>
    [HttpPost("{id}/upload-contract")]
    public async Task<IActionResult> UploadContract(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi" });

        // Boyut kontrolu
        if (file.Length > MaxFileSize)
            return BadRequest(new { message = "Dosya boyutu 10MB'i gecemez" });

        // Extension kontrolu
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
            return BadRequest(new { message = "Sadece PDF dosyalari yuklenebilir" });

        // Content-Type kontrolu
        if (file.ContentType != "application/pdf")
            return BadRequest(new { message = "Gecersiz dosya formati" });

        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        // Eski dosyayi sil (varsa)
        if (!string.IsNullOrEmpty(company.ContractFileUrl))
        {
            var oldFilePath = Path.Combine(_env.WebRootPath, company.ContractFileUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }

        // Dosya adini olustur (guvenli, unique)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var fileName = $"contract_{id}_{timestamp}.pdf";
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "contracts");

        // Klasor yoksa olustur
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var filePath = Path.Combine(uploadsPath, fileName);

        // Dosyayi kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Veritabanini guncelle
        company.ContractFileUrl = $"/uploads/contracts/{fileName}";
        company.ContractUploadedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Sozlesme basariyla yuklendi",
            contractFileUrl = company.ContractFileUrl,
            contractUploadedAt = company.ContractUploadedAt
        });
    }

    /// <summary>
    /// Firma sozlesmesini sil
    /// </summary>
    [HttpDelete("{id}/contract")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        if (string.IsNullOrEmpty(company.ContractFileUrl))
            return BadRequest(new { message = "Silinecek sozlesme bulunamadi" });

        // Dosyayi sil
        var filePath = Path.Combine(_env.WebRootPath, company.ContractFileUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        // Veritabanini guncelle
        company.ContractFileUrl = string.Empty;
        company.ContractUploadedAt = null;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Sozlesme basariyla silindi" });
    }

    // ===============================================
    // FIRMA GALERI ISLEMLERI
    // ===============================================

    /// <summary>
    /// Firma galerisine resim yukle
    /// </summary>
    [HttpPost("{id}/gallery")]
    public async Task<ActionResult<object>> UploadGalleryImage(int id, [FromForm] IFormFile file, [FromForm] string? title = null, [FromForm] string? description = null, [FromForm] bool isFeatured = false)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        // Maksimum 20 resim
        var imageCount = await _context.CompanyGalleryImages.CountAsync(g => g.CompanyId == id && g.IsActive);
        if (imageCount >= 20)
            return BadRequest(new { message = "Galeriye en fazla 20 resim yuklenebilir" });

        // Dosya kontrolu
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi" });

        // Yukleme
        using var stream = file.OpenReadStream();
        var result = await _fileUploadService.UploadImageWithThumbnailAsync(stream, file.FileName, "galleries");

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        // Eger isFeatured ise diger featured resimlerin durumunu guncelle
        if (isFeatured)
        {
            await _context.CompanyGalleryImages
                .Where(g => g.CompanyId == id && g.IsFeatured)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.IsFeatured, false));
        }

        // Veritabanina kaydet
        var galleryImage = new CompanyGalleryImage
        {
            CompanyId = id,
            ImageUrl = result.Url!,
            ThumbnailUrl = result.ThumbnailUrl ?? result.Url!,
            Title = title ?? string.Empty,
            Description = description ?? string.Empty,
            DisplayOrder = imageCount + 1,
            FileSize = result.FileSize,
            Width = result.Width,
            Height = result.Height,
            MimeType = result.MimeType ?? "image/jpeg",
            AltText = title ?? $"Galeri resmi {imageCount + 1}",
            IsFeatured = isFeatured
        };

        _context.CompanyGalleryImages.Add(galleryImage);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Resim yuklendi",
            image = new
            {
                galleryImage.Id,
                galleryImage.ImageUrl,
                galleryImage.ThumbnailUrl,
                galleryImage.Title,
                galleryImage.Description,
                galleryImage.DisplayOrder,
                galleryImage.IsFeatured
            }
        });
    }

    /// <summary>
    /// Firma galeri resimlerini listele
    /// </summary>
    [HttpGet("{id}/gallery")]
    public async Task<ActionResult<object>> GetGalleryImages(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Firma bulunamadi" });

        var images = await _context.CompanyGalleryImages
            .Where(g => g.CompanyId == id && g.IsActive)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new
            {
                g.Id,
                g.ImageUrl,
                g.ThumbnailUrl,
                g.Title,
                g.Description,
                g.DisplayOrder,
                g.Width,
                g.Height,
                g.IsFeatured
            })
            .ToListAsync();

        return Ok(images);
    }

    /// <summary>
    /// Galeri resmini guncelle
    /// </summary>
    [HttpPut("{companyId}/gallery/{imageId}")]
    public async Task<IActionResult> UpdateGalleryImage(int companyId, int imageId, [FromBody] UpdateGalleryImageRequest request)
    {
        var image = await _context.CompanyGalleryImages.FirstOrDefaultAsync(g => g.Id == imageId && g.CompanyId == companyId && g.IsActive);
        if (image == null)
            return NotFound(new { message = "Resim bulunamadi" });

        if (request.Title != null) image.Title = request.Title;
        if (request.Description != null) image.Description = request.Description;
        if (request.AltText != null) image.AltText = request.AltText;
        if (request.DisplayOrder.HasValue) image.DisplayOrder = request.DisplayOrder.Value;

        if (request.IsFeatured.HasValue && request.IsFeatured.Value)
        {
            // Diger featured'lari kaldir
            await _context.CompanyGalleryImages
                .Where(g => g.CompanyId == companyId && g.IsFeatured && g.Id != imageId)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.IsFeatured, false));
            image.IsFeatured = true;
        }
        else if (request.IsFeatured.HasValue)
        {
            image.IsFeatured = false;
        }

        image.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Resim guncellendi" });
    }

    /// <summary>
    /// Galeri resmini sil
    /// </summary>
    [HttpDelete("{companyId}/gallery/{imageId}")]
    public async Task<IActionResult> DeleteGalleryImage(int companyId, int imageId)
    {
        var image = await _context.CompanyGalleryImages.FirstOrDefaultAsync(g => g.Id == imageId && g.CompanyId == companyId);
        if (image == null)
            return NotFound(new { message = "Resim bulunamadi" });

        // Dosyalari sil
        await _fileUploadService.DeleteFileAsync(image.ImageUrl);
        if (!string.IsNullOrEmpty(image.ThumbnailUrl))
            await _fileUploadService.DeleteFileAsync(image.ThumbnailUrl);

        // Veritabanindan sil (soft delete)
        image.IsActive = false;
        image.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Resim silindi" });
    }

    /// <summary>
    /// Galeri resim siralamasini guncelle
    /// </summary>
    [HttpPut("{id}/gallery/reorder")]
    public async Task<IActionResult> ReorderGalleryImages(int id, [FromBody] ReorderGalleryRequest request)
    {
        if (request.ImageIds == null || !request.ImageIds.Any())
            return BadRequest(new { message = "Resim listesi bos olamaz" });

        var images = await _context.CompanyGalleryImages
            .Where(g => g.CompanyId == id && request.ImageIds.Contains(g.Id) && g.IsActive)
            .ToListAsync();

        for (int i = 0; i < request.ImageIds.Count; i++)
        {
            var image = images.FirstOrDefault(img => img.Id == request.ImageIds[i]);
            if (image != null)
            {
                image.DisplayOrder = i + 1;
                image.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Siralama guncellendi" });
    }
}

// Request DTO'lari
public class ApproveCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string? ReviewNotes { get; set; }
}

public class RejectCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
}

public class SuspendCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReactivateCompanyRequest
{
    public int? ReviewedById { get; set; }
    public string? ReviewNotes { get; set; }
}

public class UpdateGalleryImageRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AltText { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsFeatured { get; set; }
}

public class ReorderGalleryRequest
{
    public List<int> ImageIds { get; set; } = new();
}

public class UpdateCompanySettingsRequest
{
    public int DepositPercentage { get; set; }
}
