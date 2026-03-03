using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Factories.Tours;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Tours;

/// <summary>
/// Tur paketi olusturucu (Faz 15.6)
/// </summary>
public class TourPackageFactory : ITourPackageFactory
{
    private readonly ITourPackageEntityService _packageService;
    private readonly ITourEntityService _tourService;
    private readonly IUnitOfWork _unitOfWork;

    public TourPackageFactory(
        ITourPackageEntityService packageService,
        ITourEntityService tourService,
        IUnitOfWork unitOfWork)
    {
        _packageService = packageService;
        _tourService = tourService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, object result, int statusCode)> GetMyPackagesAsync(int visitorId)
    {
        var packages = await _packageService.GetByVisitorId(visitorId)
            .Include(p => p.Items).ThenInclude(i => i.Tour)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapPackage(p))
            .ToListAsync();

        return (true, packages, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetPublicPackagesAsync(int page = 1, int pageSize = 20)
    {
        var query = _packageService.GetActivePackages()
            .Where(p => p.IsPublic)
            .Include(p => p.Items).ThenInclude(i => i.Tour)
            .Include(p => p.Visitor);

        var totalCount = await query.CountAsync();
        var packages = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                createdBy = p.Visitor.FirstName,
                p.TotalPrice,
                p.DiscountPercent,
                p.FinalPrice,
                tourCount = p.Items.Count,
                tours = p.Items.OrderBy(i => i.SortOrder).Select(i => new
                {
                    i.TourId,
                    tourName = i.Tour.Name,
                    destination = i.Tour.Destination,
                    price = i.Tour.Price,
                    i.NumberOfPeople
                }).ToList(),
                p.CreatedAt
            })
            .ToListAsync();

        return (true, new { items = packages, totalCount, page, pageSize }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetPackageByIdAsync(int packageId)
    {
        var package = await _packageService.GetByIdAsync(packageId);
        if (package == null)
            return (false, new { message = "Paket bulunamadi" }, 404);

        return (true, new
        {
            package.Id,
            package.Name,
            package.Description,
            createdBy = package.Visitor?.FirstName,
            package.TotalPrice,
            package.DiscountPercent,
            package.FinalPrice,
            package.IsPublic,
            tours = package.Items.OrderBy(i => i.SortOrder).Select(i => new
            {
                i.Id,
                i.TourId,
                tourName = i.Tour.Name,
                destination = i.Tour.Destination,
                price = i.Tour.Price,
                imageUrl = i.Tour.ImageUrl,
                i.NumberOfPeople,
                i.SortOrder
            }).ToList(),
            package.CreatedAt
        }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> CreatePackageAsync(
        int visitorId, string name, string? description, bool isPublic, List<PackageItemInput> items)
    {
        if (items == null || items.Count < 2)
            return (false, new { message = "Paket en az 2 tur icermelidir" }, 400);

        // Turlari dogrula ve fiyat hesapla
        decimal totalPrice = 0;
        var validatedItems = new List<(int tourId, decimal price, int people)>();

        for (int i = 0; i < items.Count; i++)
        {
            var tour = await _tourService.GetByIdAsync(items[i].TourId);
            if (tour == null || !tour.IsActive)
                return (false, new { message = $"Tur bulunamadi: {items[i].TourId}" }, 400);

            var people = Math.Max(1, items[i].NumberOfPeople);
            totalPrice += tour.Price * people;
            validatedItems.Add((tour.Id, tour.Price, people));
        }

        // Paket indirimi: 2 tur %5, 3 tur %8, 4+ tur %12
        var discountPercent = items.Count switch
        {
            2 => 5m,
            3 => 8m,
            _ => 12m
        };

        var finalPrice = totalPrice * (1 - discountPercent / 100);

        var package = new TourPackage
        {
            Name = name,
            Description = description,
            VisitorId = visitorId,
            TotalPrice = totalPrice,
            DiscountPercent = discountPercent,
            FinalPrice = Math.Round(finalPrice, 2),
            IsPublic = isPublic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _packageService.Add(package);
        await _unitOfWork.SaveChangesAsync();

        // Item'lari ekle
        for (int i = 0; i < validatedItems.Count; i++)
        {
            var item = new TourPackageItem
            {
                TourPackageId = package.Id,
                TourId = validatedItems[i].tourId,
                SortOrder = i,
                NumberOfPeople = validatedItems[i].people,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _packageService.AddItem(item);
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, new
        {
            package.Id,
            package.Name,
            package.TotalPrice,
            package.DiscountPercent,
            package.FinalPrice,
            tourCount = validatedItems.Count
        }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> DeletePackageAsync(int visitorId, int packageId)
    {
        var package = await _packageService.GetByIdAsync(packageId);
        if (package == null)
            return (false, new { message = "Paket bulunamadi" }, 404);

        if (package.VisitorId != visitorId)
            return (false, new { message = "Bu paket size ait degil" }, 403);

        package.IsActive = false;
        package.UpdatedAt = DateTime.UtcNow;
        _packageService.Update(package);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Paket silindi" }, 200);
    }

    private static object MapPackage(TourPackage p) => new
    {
        p.Id,
        p.Name,
        p.Description,
        p.TotalPrice,
        p.DiscountPercent,
        p.FinalPrice,
        p.IsPublic,
        tourCount = p.Items.Count,
        tours = p.Items.OrderBy(i => i.SortOrder).Select(i => new
        {
            i.TourId,
            tourName = i.Tour.Name,
            destination = i.Tour.Destination,
            price = i.Tour.Price,
            i.NumberOfPeople
        }).ToList(),
        p.CreatedAt
    };
}
