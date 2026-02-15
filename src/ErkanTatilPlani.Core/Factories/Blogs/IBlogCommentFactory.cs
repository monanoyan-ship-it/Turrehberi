namespace ErkanTatilPlani.Core.Factories.Blogs;

public interface IBlogCommentFactory
{
    Task<object> GetCommentsAsync(int postId);
    Task<(bool success, string? errorMessage, string? errorCode, int? statusCode)> AddCommentAsync(int visitorId, int postId, string content, int? parentCommentId);
    Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> DeleteCommentAsync(int visitorId, int commentId);
    Task<(bool success, bool notFound, string? errorMessage, string? errorCode, int? statusCode)> AdminDeleteCommentAsync(int commentId);
}
