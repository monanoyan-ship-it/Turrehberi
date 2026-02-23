using System.Text.RegularExpressions;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Companies;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Companies;

public class CompanyPageFactory : ICompanyPageFactory
{
    private readonly ICompanyPageEntityService _pageService;
    private readonly ICompanyEntityService _companyService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyPageFactory(
        ICompanyPageEntityService pageService,
        ICompanyEntityService companyService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _pageService = pageService;
        _companyService = companyService;
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    private async Task<(bool isOwner, string? errorMessage, string? errorCode, int? statusCode)> ValidateCompanyOwnerAsync(int visitorId, int companyId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null)
            return (false, "Error.UserNotFound", null, 401);

        if (visitor.UserTypeId < UserTypes.Ids.Staff && visitor.CompanyId != companyId)
            return (false, "Error.CompanyAccessDenied", "NO_PERMISSION", 403);

        return (true, null, null, null);
    }

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetPagesAsync(int visitorId, int companyId)
    {
        var (isOwner, errorMessage, errorCode, statusCode) = await ValidateCompanyOwnerAsync(visitorId, companyId);
        if (!isOwner)
            return (null, errorMessage, errorCode, statusCode);

        var pages = await _pageService.GetByCompanyId(companyId)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Icon, p.DisplayOrder, p.IsPublished,
                p.MetaTitle, p.MetaDescription, p.CreatedAt, p.UpdatedAt
            })
            .ToListAsync();

        return (pages, null, null, null);
    }

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetPageAsync(int visitorId, int companyId, int id)
    {
        var (isOwner, errorMessage, errorCode, statusCode) = await ValidateCompanyOwnerAsync(visitorId, companyId);
        if (!isOwner)
            return (null, errorMessage, errorCode, statusCode);

        var page = await _pageService.GetByIdAsync(id);
        if (page == null || page.CompanyId != companyId || !page.IsActive)
            return (null, "Error.PageNotFound", null, 404);

        return (new
        {
            page.Id, page.Title, page.Slug, page.Content, page.Icon, page.DisplayOrder,
            page.IsPublished, page.MetaTitle, page.MetaDescription, page.CreatedAt, page.UpdatedAt
        }, null, null, null);
    }

    public async Task<(CompanyPage? page, string? errorMessage, string? errorCode, int? statusCode)> CreatePageAsync(int visitorId, int companyId, CompanyPage page)
    {
        var (isOwner, errorMessage, errorCode, statusCode) = await ValidateCompanyOwnerAsync(visitorId, companyId);
        if (!isOwner)
            return (null, errorMessage, errorCode, statusCode);

        page.CompanyId = companyId;
        page.Slug = await GenerateUniqueSlugAsync(companyId, page.Title);
        page.CreatedAt = DateTime.UtcNow;

        // DisplayOrder: en sona ekle
        var maxOrder = await _pageService.GetByCompanyId(companyId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;
        page.DisplayOrder = maxOrder + 1;

        _pageService.Add(page);
        await _unitOfWork.SaveChangesAsync();
        return (page, null, null, null);
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdatePageAsync(int visitorId, int companyId, int id, CompanyPage page)
    {
        var (isOwner, errorMessage, errorCode, statusCode) = await ValidateCompanyOwnerAsync(visitorId, companyId);
        if (!isOwner)
            return (false, errorMessage, errorCode, statusCode);

        var existing = await _pageService.GetByIdAsync(id);
        if (existing == null || existing.CompanyId != companyId || !existing.IsActive)
            return (false, "Error.PageNotFound", null, 404);

        existing.Title = page.Title;
        existing.Content = page.Content;
        existing.Icon = page.Icon;
        existing.IsPublished = page.IsPublished;
        existing.MetaTitle = page.MetaTitle;
        existing.MetaDescription = page.MetaDescription;
        existing.UpdatedAt = DateTime.UtcNow;

        // Slug guncelle sadece yayinda degilse
        if (!existing.IsPublished)
            existing.Slug = await GenerateUniqueSlugAsync(companyId, page.Title, existing.Id);

        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> DeletePageAsync(int visitorId, int companyId, int id)
    {
        var (isOwner, errorMessage, errorCode, statusCode) = await ValidateCompanyOwnerAsync(visitorId, companyId);
        if (!isOwner)
            return (false, errorMessage, errorCode, statusCode);

        var page = await _pageService.GetByIdAsync(id);
        if (page == null || page.CompanyId != companyId || !page.IsActive)
            return (false, "Error.PageNotFound", null, 404);

        page.IsActive = false;
        page.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> ReorderPagesAsync(int visitorId, int companyId, List<int> pageIds)
    {
        var (isOwner, errorMessage, errorCode, statusCode) = await ValidateCompanyOwnerAsync(visitorId, companyId);
        if (!isOwner)
            return (false, errorMessage, errorCode, statusCode);

        var pages = await _pageService.GetByCompanyId(companyId).ToListAsync();

        for (int i = 0; i < pageIds.Count; i++)
        {
            var page = pages.FirstOrDefault(p => p.Id == pageIds[i]);
            if (page != null)
            {
                page.DisplayOrder = i + 1;
                page.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<object?> GetPublicPagesAsync(string companySlug)
    {
        var company = await _companyService.GetBySlugAsync(companySlug);
        if (company == null || !company.IsActive || company.StatusId != CompanyStatuses.Ids.Approved)
            return null;

        return await _pageService.GetByCompanyId(company.Id)
            .Where(p => p.IsPublished)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Icon, p.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<object?> GetPublicPageBySlugAsync(string companySlug, string pageSlug)
    {
        var company = await _companyService.GetBySlugAsync(companySlug);
        if (company == null || !company.IsActive || company.StatusId != CompanyStatuses.Ids.Approved)
            return null;

        var page = await _pageService.GetBySlugAsync(company.Id, pageSlug);
        if (page == null || !page.IsPublished)
            return null;

        return new
        {
            page.Id, page.Title, page.Slug, page.Content, page.Icon,
            page.MetaTitle, page.MetaDescription,
            CompanyId = company.Id,
            CompanyName = company.Name,
            CompanySlug = company.Slug,
            CompanyLogo = company.LogoUrl
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(int companyId, string title, int? excludeId = null)
    {
        var slug = GenerateSlug(title);
        var existingPage = await _pageService.GetBySlugAsync(companyId, slug);

        if (existingPage == null || (excludeId.HasValue && existingPage.Id == excludeId.Value))
            return slug;

        // Slug zaten varsa sonuna numara ekle
        var counter = 2;
        while (true)
        {
            var newSlug = $"{slug}-{counter}";
            existingPage = await _pageService.GetBySlugAsync(companyId, newSlug);
            if (existingPage == null || (excludeId.HasValue && existingPage.Id == excludeId.Value))
                return newSlug;
            counter++;
        }
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();

        // Turkce karakter donusumu
        slug = slug.Replace("\u00e7", "c").Replace("\u011f", "g").Replace("\u0131", "i")
                   .Replace("\u00f6", "o").Replace("\u015f", "s").Replace("\u00fc", "u")
                   .Replace("\u00c7", "c").Replace("\u011e", "g").Replace("\u0130", "i")
                   .Replace("\u00d6", "o").Replace("\u015e", "s").Replace("\u00dc", "u");

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }
}
