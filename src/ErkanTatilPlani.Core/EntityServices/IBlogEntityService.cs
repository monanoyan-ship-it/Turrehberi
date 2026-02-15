using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IBlogEntityService
{
    IQueryable<BlogPost> GetActivePosts();
    IQueryable<BlogPost> GetPublishedPosts();
    Task<BlogPost?> GetByIdAsync(int id);
    Task<BlogPost?> GetBySlugAsync(string slug);
    void Add(BlogPost post);
    void Update(BlogPost post);
    IQueryable<BlogComment> GetActiveComments();
    IQueryable<BlogComment> GetCommentsByPostId(int postId);
    void AddComment(BlogComment comment);
}
