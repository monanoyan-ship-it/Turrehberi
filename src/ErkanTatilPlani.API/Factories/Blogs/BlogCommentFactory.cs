using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Blogs;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Blogs;

public class BlogCommentFactory : IBlogCommentFactory
{
    private readonly IBlogEntityService _blogService;
    private readonly IVisitorEntityService _visitorService;
    private readonly IUnitOfWork _unitOfWork;

    public BlogCommentFactory(
        IBlogEntityService blogService,
        IVisitorEntityService visitorService,
        IUnitOfWork unitOfWork)
    {
        _blogService = blogService;
        _visitorService = visitorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetCommentsAsync(int postId)
    {
        var comments = await _blogService.GetCommentsByPostId(postId)
            .Where(c => c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                VisitorId = c.VisitorId,
                VisitorName = c.Visitor.FirstName + " " + c.Visitor.LastName,
                VisitorAvatar = c.Visitor.AvatarUrl,
                Replies = c.Replies.Where(r => r.IsActive).OrderBy(r => r.CreatedAt).Select(r => new
                {
                    r.Id,
                    r.Content,
                    r.CreatedAt,
                    VisitorId = r.VisitorId,
                    VisitorName = r.Visitor.FirstName + " " + r.Visitor.LastName,
                    VisitorAvatar = r.Visitor.AvatarUrl
                })
            })
            .ToListAsync();

        return comments;
    }

    public async Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> AddCommentAsync(int visitorId, int postId, string content, int? parentCommentId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return (false, "Error.UserNotFound", null, 401);

        var post = await _blogService.GetByIdAsync(postId);
        if (post == null || !post.IsActive || post.StatusId != BlogStatuses.Ids.Published)
            return (false, "Error.PostNotFound", null, 404);

        if (parentCommentId.HasValue)
        {
            var parent = await _blogService.GetActiveComments()
                .FirstOrDefaultAsync(c => c.Id == parentCommentId.Value && c.BlogPostId == postId);
            if (parent == null) return (false, "Error.ParentCommentNotFound", null, 404);
        }

        var comment = new BlogComment
        {
            BlogPostId = postId,
            VisitorId = visitorId,
            Content = content,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        _blogService.AddComment(comment);
        await _unitOfWork.SaveChangesAsync();
        return (true, null, null, null);
    }

    public async Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeleteCommentAsync(int visitorId, int commentId)
    {
        var comment = await _blogService.GetActiveComments()
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment == null) return (false, true, null, null, null);

        if (comment.VisitorId != visitorId)
            return (false, false, "Error.CommentDeletePermissionDenied", "NO_PERMISSION", 403);

        comment.IsActive = false;
        comment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, false, null, null, null);
    }

    public async Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> AdminDeleteCommentAsync(int commentId)
    {
        var comment = await _blogService.GetActiveComments()
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment == null) return (false, true, null, null, null);

        comment.IsActive = false;
        comment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return (true, false, null, null, null);
    }
}
