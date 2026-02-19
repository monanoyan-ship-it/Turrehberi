using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Factories.Blogs;

public interface IBlogFactory
{
    Task<object> GetPublishedPostsAsync(int? categoryId, string? search, int page, int pageSize);
    Task<object?> GetPostBySlugAsync(string slug);
    Task<object> GetCategoriesAsync();
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetMyPostsAsync(int visitorId);
    Task<(BlogPost? post, string? errorMessage, string? errorCode, int? statusCode)> CreatePostAsync(int visitorId, BlogPost post);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdatePostAsync(int visitorId, int id, BlogPost post);
    Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeletePostAsync(int visitorId, int id);
    Task<object> GetAllPostsAsync(int? statusId, int? categoryId, string? search, int page, int pageSize);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> UpdatePostStatusAsync(int id, int statusId);

    // Firma blog metodlari
    Task<object> GetCompanyBlogPostsAsync(int companyId, int page, int pageSize);
    Task<(object? result, string? errorMessage, string? errorCode, int? statusCode)> GetCompanyDraftPostsAsync(int visitorId, int companyId);
}
