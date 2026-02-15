using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class BlogEntityService : IBlogEntityService
{
    private readonly AppDbContext _context;

    public BlogEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<BlogPost> GetActivePosts()
        => _context.BlogPosts.Where(p => p.IsActive);

    public IQueryable<BlogPost> GetPublishedPosts()
        => _context.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Company)
            .Where(p => p.IsActive && p.StatusId == BlogStatuses.Ids.Published);

    public async Task<BlogPost?> GetByIdAsync(int id)
        => await _context.BlogPosts.FindAsync(id);

    public async Task<BlogPost?> GetBySlugAsync(string slug)
        => await _context.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Company)
            .Include(p => p.Comments.Where(c => c.IsActive))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

    public void Add(BlogPost post) => _context.BlogPosts.Add(post);

    public void Update(BlogPost post) => _context.Entry(post).State = EntityState.Modified;

    public IQueryable<BlogComment> GetActiveComments()
        => _context.BlogComments.Where(c => c.IsActive);

    public IQueryable<BlogComment> GetCommentsByPostId(int postId)
        => _context.BlogComments
            .Include(c => c.Visitor)
            .Where(c => c.BlogPostId == postId && c.IsActive);

    public void AddComment(BlogComment comment) => _context.BlogComments.Add(comment);
}
