using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Tours;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Tours;

/// <summary>
/// Tur sonrasi dijital icerik yonetimi (Faz 15.7)
/// </summary>
public class TourDigitalContentFactory : ITourDigitalContentFactory
{
    private readonly ITourDigitalContentEntityService _contentService;
    private readonly ITourEntityService _tourService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IReservationEntityService _reservationService;
    private readonly IUnitOfWork _unitOfWork;

    public TourDigitalContentFactory(
        ITourDigitalContentEntityService contentService,
        ITourEntityService tourService,
        IVisitorEntityService visitorService,
        IReservationEntityService reservationService,
        IUnitOfWork unitOfWork)
    {
        _contentService = contentService;
        _tourService = tourService;
        _visitorService = visitorService;
        _reservationService = reservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object result, int statusCode)> GetTourContentsAsync(int tourId, int? visitorId)
    {
        var tour = await _tourService.GetByIdAsync(tourId);
        if (tour == null)
            return (false, new { message = "Tur bulunamadi" }, 404);

        var contents = await _contentService.GetByTourId(tourId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        // Ziyaretci rezervasyon yapmis mi kontrol et
        bool hasReservation = false;
        if (visitorId.HasValue)
        {
            hasReservation = await _reservationService.GetByVisitorId(visitorId.Value)
                .AnyAsync(r => r.TourId == tourId &&
                    (r.Status == ReservationStatuses.Ids.Confirmed || r.Status == ReservationStatuses.Ids.Completed));
        }

        var result = contents.Select(c => new
        {
            c.Id,
            c.Title,
            c.Description,
            c.ContentTypeId,
            contentTypeName = DigitalContentTypes.GetById(c.ContentTypeId)?.SystemName,
            contentTypeIcon = DigitalContentTypes.GetById(c.ContentTypeId)?.Icon,
            fileUrl = c.RequiresReservation && !hasReservation ? null : c.FileUrl,
            content = c.RequiresReservation && !hasReservation ? null : c.Content,
            c.RequiresReservation,
            isLocked = c.RequiresReservation && !hasReservation,
            c.SortOrder
        }).ToList();

        return (true, new { tourId, tourName = tour.Name, hasReservation, contents = result }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> ManageContentAsync(
        int visitorId, int tourId, int? contentId, string title, string? description,
        int contentTypeId, string? fileUrl, string? content, int sortOrder, bool requiresReservation)
    {
        // Firma sahibi kontrolu
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.UserTypeId != UserTypes.Ids.CompanyOwner)
            return (false, new { message = "Yetkiniz yok" }, 403);

        var tour = await _tourService.GetByIdWithCompanyAsync(tourId);
        if (tour == null || tour.CompanyId != visitor.CompanyId)
            return (false, new { message = "Bu tur size ait degil" }, 403);

        if (contentId.HasValue)
        {
            // Guncelle
            var existing = await _contentService.GetByIdAsync(contentId.Value);
            if (existing == null || existing.TourId != tourId)
                return (false, new { message = "Icerik bulunamadi" }, 404);

            existing.Title = title;
            existing.Description = description;
            existing.ContentTypeId = contentTypeId;
            existing.FileUrl = fileUrl;
            existing.Content = content;
            existing.SortOrder = sortOrder;
            existing.RequiresReservation = requiresReservation;
            existing.UpdatedAt = DateTime.UtcNow;

            _contentService.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return (true, new { message = "Icerik guncellendi", id = existing.Id }, 200);
        }
        else
        {
            // Yeni olustur
            var newContent = new TourDigitalContent
            {
                TourId = tourId,
                Title = title,
                Description = description,
                ContentTypeId = contentTypeId,
                FileUrl = fileUrl,
                Content = content,
                SortOrder = sortOrder,
                RequiresReservation = requiresReservation,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _contentService.Add(newContent);
            await _unitOfWork.SaveChangesAsync();

            return (true, new { message = "Icerik olusturuldu", id = newContent.Id }, 200);
        }
    }

    public async Task<(bool success, object result, int statusCode)> DeleteContentAsync(int visitorId, int contentId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null || visitor.UserTypeId != UserTypes.Ids.CompanyOwner)
            return (false, new { message = "Yetkiniz yok" }, 403);

        var content = await _contentService.GetByIdAsync(contentId);
        if (content == null)
            return (false, new { message = "Icerik bulunamadi" }, 404);

        var tour = await _tourService.GetByIdWithCompanyAsync(content.TourId);
        if (tour == null || tour.CompanyId != visitor.CompanyId)
            return (false, new { message = "Bu icerik size ait degil" }, 403);

        content.IsActive = false;
        content.UpdatedAt = DateTime.UtcNow;
        _contentService.Update(content);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Icerik silindi" }, 200);
    }
}
