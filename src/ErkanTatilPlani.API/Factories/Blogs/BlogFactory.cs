using System.Text.RegularExpressions;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Blogs;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Blogs;

public class BlogFactory : IBlogFactory
{
    private readonly IBlogEntityService _blogService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public BlogFactory(
        IBlogEntityService blogService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _blogService = blogService;
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetPublishedPostsAsync(int? categoryId, string? search, int page, int pageSize)
    {
        var query = _blogService.GetPublishedPosts();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(searchLower) ||
                                     p.Summary.ToLower().Contains(searchLower) ||
                                     (p.Tags != null && p.Tags.ToLower().Contains(searchLower)));
        }

        query = query.OrderByDescending(p => p.PublishedAt);

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Summary, p.ImageUrl, p.CategoryId, p.ViewCount,
                p.PublishedAt, p.Tags,
                AuthorName = p.Author.FirstName + " " + p.Author.LastName,
                CompanyName = p.Company != null ? p.Company.Name : null,
                CommentCount = p.Comments.Count(c => c.IsActive)
            })
            .ToListAsync();

        return new
        {
            posts,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<object?> GetPostBySlugAsync(string slug)
    {
        var post = await _blogService.GetBySlugAsync(slug);
        if (post == null || post.StatusId != BlogStatuses.Ids.Published) return null;

        // ViewCount artir
        post.ViewCount++;
        await _unitOfWork.SaveChangesAsync();

        return new
        {
            post.Id, post.Title, post.Slug, post.Summary, post.Content, post.ImageUrl,
            post.CategoryId, post.StatusId, post.ViewCount, post.PublishedAt, post.Tags,
            post.MetaTitle, post.MetaDescription,
            AuthorName = post.Author.FirstName + " " + post.Author.LastName,
            AuthorAvatar = post.Author.AvatarUrl,
            CompanyName = post.Company?.Name,
            CompanySlug = post.Company?.Slug,
            CommentCount = post.Comments.Count(c => c.IsActive)
        };
    }

    public async Task<object> GetCategoriesAsync()
    {
        var categoryCounts = await _blogService.GetPublishedPosts()
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync();

        return BlogCategories.All.Select(c => new
        {
            c.Id, c.SystemName, c.NameResourceKey, c.Icon, c.CssClass,
            PostCount = categoryCounts.FirstOrDefault(x => x.CategoryId == c.Id)?.Count ?? 0
        });
    }

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetMyPostsAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null)
            return (null, "Error.UserNotFound", null, 401);

        if (visitor.UserTypeId < UserTypes.Ids.CompanyOwner)
            return (null, "Error.BlogWritePermissionRequired", "NO_PERMISSION", 403);

        var query = _blogService.GetActivePosts()
            .Where(p => p.AuthorId == visitorId);

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Summary, p.ImageUrl, p.CategoryId, p.StatusId,
                p.ViewCount, p.PublishedAt, p.Tags, p.CreatedAt,
                CommentCount = p.Comments.Count(c => c.IsActive)
            })
            .ToListAsync();

        return (posts, null, null, null);
    }

    public async Task<(BlogPost? post, string? errorMessage, string? errorCode, int? statusCode)> CreatePostAsync(int visitorId, BlogPost post)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null) return (null, "Error.UserNotFound", null, 401);
        if (visitor.UserTypeId < UserTypes.Ids.CompanyOwner)
            return (null, "Error.BlogWritePermissionDenied", "NO_PERMISSION", 403);

        post.AuthorId = visitorId;
        post.CompanyId = visitor.CompanyId;
        post.Slug = GenerateSlug(post.Title);
        post.CreatedAt = DateTime.UtcNow;

        if (post.StatusId == BlogStatuses.Ids.Published)
            post.PublishedAt = DateTime.UtcNow;

        _blogService.Add(post);
        await _unitOfWork.SaveChangesAsync();
        return (post, null, null, null);
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdatePostAsync(int visitorId, int id, BlogPost post)
    {
        var existing = await _blogService.GetByIdAsync(id);
        if (existing == null) return (false, "Error.PostNotFound", null, 404);
        if (existing.AuthorId != visitorId)
        {
            var visitor = await _visitorService.GetByIdAsync(visitorId);
            if (visitor == null || visitor.UserTypeId < UserTypes.Ids.Staff)
                return (false, "Error.PostEditPermissionDenied", "NO_PERMISSION", 403);
        }

        existing.Title = post.Title;
        existing.Summary = post.Summary;
        existing.Content = post.Content;
        existing.ImageUrl = post.ImageUrl;
        existing.CategoryId = post.CategoryId;
        existing.Tags = post.Tags;
        existing.MetaTitle = post.MetaTitle;
        existing.MetaDescription = post.MetaDescription;
        existing.UpdatedAt = DateTime.UtcNow;

        // Slug guncelle sadece taslaksa
        if (existing.StatusId == BlogStatuses.Ids.Draft)
            existing.Slug = GenerateSlug(post.Title);

        // Durum degistiyse ve yayina alindiysa publishedAt set et
        if (post.StatusId == BlogStatuses.Ids.Published && existing.StatusId != BlogStatuses.Ids.Published)
            existing.PublishedAt = DateTime.UtcNow;

        existing.StatusId = post.StatusId;

        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeletePostAsync(int visitorId, int id)
    {
        var post = await _blogService.GetByIdAsync(id);
        if (post == null) return (false, true, null, null, null);

        if (post.AuthorId != visitorId)
        {
            var visitor = await _visitorService.GetByIdAsync(visitorId);
            if (visitor == null || visitor.UserTypeId < UserTypes.Ids.Staff)
                return (false, false, "Error.PostDeletePermissionDenied", "NO_PERMISSION", 403);
        }

        post.IsActive = false;
        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, false, null, null, null);
    }

    public async Task<object> GetAllPostsAsync(int? statusId, int? categoryId, string? search, int page, int pageSize)
    {
        IQueryable<BlogPost> query = _blogService.GetActivePosts()
            .Include(p => p.Author);

        if (statusId.HasValue)
            query = query.Where(p => p.StatusId == statusId.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Summary, p.ImageUrl, p.CategoryId, p.StatusId,
                p.ViewCount, p.PublishedAt, p.Tags, p.CreatedAt,
                AuthorName = p.Author.FirstName + " " + p.Author.LastName,
                CommentCount = p.Comments.Count(c => c.IsActive)
            })
            .ToListAsync();

        return new { posts, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdatePostStatusAsync(int id, int statusId)
    {
        var post = await _blogService.GetByIdAsync(id);
        if (post == null) return (false, "Error.PostNotFound", null, 404);

        if (statusId == BlogStatuses.Ids.Published && post.StatusId != BlogStatuses.Ids.Published)
            post.PublishedAt = DateTime.UtcNow;

        post.StatusId = statusId;
        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<object> GetCompanyBlogPostsAsync(int companyId, int page, int pageSize)
    {
        var query = _blogService.GetPublishedPosts()
            .Where(p => p.CompanyId == companyId);

        var totalCount = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Summary, p.ImageUrl, p.CategoryId, p.ViewCount,
                p.PublishedAt, p.Tags,
                AuthorName = p.Author.FirstName + " " + p.Author.LastName,
                CommentCount = p.Comments.Count(c => c.IsActive)
            })
            .ToListAsync();

        return new { posts, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) };
    }

    public async Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetCompanyDraftPostsAsync(int visitorId, int companyId)
    {
        var visitor = await _visitorService.GetByIdWithCompanyAsync(visitorId);
        if (visitor == null)
            return (null, "Error.UserNotFound", null, 401);

        if (visitor.UserTypeId < UserTypes.Ids.Staff && visitor.CompanyId != companyId)
            return (null, "Error.CompanyAccessDenied", "NO_PERMISSION", 403);

        var posts = await _blogService.GetActivePosts()
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id, p.Title, p.Slug, p.Summary, p.ImageUrl, p.CategoryId, p.StatusId,
                p.ViewCount, p.PublishedAt, p.Tags, p.CreatedAt,
                AuthorName = p.Author.FirstName + " " + p.Author.LastName,
                CommentCount = p.Comments.Count(c => c.IsActive)
            })
            .ToListAsync();

        return (posts, null, null, null);
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();

        // Turkce karakter donusumu
        slug = slug.Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                   .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u")
                   .Replace("Ç", "c").Replace("Ğ", "g").Replace("İ", "i")
                   .Replace("Ö", "o").Replace("Ş", "s").Replace("Ü", "u");

        // Alfanumerik olmayan karakterleri tire ile degistir
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }
}
