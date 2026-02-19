using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Factories.Blogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly IBlogFactory _blogFactory;
    private readonly IBlogCommentFactory _commentFactory;

    public BlogsController(IBlogFactory blogFactory, IBlogCommentFactory commentFactory)
    {
        _blogFactory = blogFactory;
        _commentFactory = commentFactory;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    // ============================================
    // PUBLIC ENDPOINTS
    // ============================================

    [HttpGet]
    public async Task<ActionResult<object>> GetPosts(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9)
    {
        return Ok(await _blogFactory.GetPublishedPostsAsync(categoryId, search, page, pageSize));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<object>> GetCategories()
    {
        return Ok(await _blogFactory.GetCategoriesAsync());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<object>> GetPost(string slug)
    {
        var post = await _blogFactory.GetPostBySlugAsync(slug);
        if (post == null) return NotFound();
        return Ok(post);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<object>> GetComments(int id)
    {
        return Ok(await _commentFactory.GetCommentsAsync(id));
    }

    // ============================================
    // AUTHENTICATED ENDPOINTS
    // ============================================

    [HttpPost("{id:int}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _commentFactory.AddCommentAsync(
            visitorId.Value, id, request.Content, request.ParentCommentId);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(new { message = "Yorum eklendi" });
    }

    [HttpDelete("comments/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, notFound, errorMessage, errorCode, statusCode) = await _commentFactory.DeleteCommentAsync(visitorId.Value, id);
        if (notFound) return NotFound();
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyPosts()
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (result, errorMessage, errorCode, statusCode) = await _blogFactory.GetMyPostsAsync(visitorId.Value);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BlogPost>> CreatePost(BlogPost post)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (created, errorMessage, errorCode, statusCode) = await _blogFactory.CreatePostAsync(visitorId.Value, post);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return CreatedAtAction(nameof(GetPost), new { slug = created!.Slug }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, BlogPost post)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, errorMessage, errorCode, statusCode) = await _blogFactory.UpdatePostAsync(visitorId.Value, id, post);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (success, notFound, errorMessage, errorCode, statusCode) = await _blogFactory.DeletePostAsync(visitorId.Value, id);
        if (notFound) return NotFound();
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    // ============================================
    // FIRMA BLOG ENDPOINTS
    // ============================================

    [HttpGet("company/{companyId:int}")]
    public async Task<ActionResult<object>> GetCompanyPosts(
        int companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9)
    {
        return Ok(await _blogFactory.GetCompanyBlogPostsAsync(companyId, page, pageSize));
    }

    [HttpGet("company/{companyId:int}/manage")]
    [Authorize]
    public async Task<IActionResult> GetCompanyDraftPosts(int companyId)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized(new { message = "Giris yapmaniz gerekiyor" });

        var (result, errorMessage, errorCode, statusCode) = await _blogFactory.GetCompanyDraftPostsAsync(visitorId.Value, companyId);
        if (errorMessage != null)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return Ok(result);
    }

    // ============================================
    // ADMIN ENDPOINTS
    // ============================================

    [HttpGet("admin")]
    [Authorize]
    public async Task<ActionResult<object>> GetAllPosts(
        [FromQuery] int? statusId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _blogFactory.GetAllPostsAsync(statusId, categoryId, search, page, pageSize));
    }

    [HttpPut("admin/{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdatePostStatus(int id, [FromBody] BlogStatusRequest request)
    {
        var (success, errorMessage, errorCode, statusCode) = await _blogFactory.UpdatePostStatusAsync(id, request.StatusId);
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }

    [HttpDelete("admin/comments/{id}")]
    [Authorize]
    public async Task<IActionResult> AdminDeleteComment(int id)
    {
        var (success, notFound, errorMessage, errorCode, statusCode) = await _commentFactory.AdminDeleteCommentAsync(id);
        if (notFound) return NotFound();
        if (!success)
        {
            var error = errorCode != null ? new { message = errorMessage, code = errorCode } : (object)new { message = errorMessage };
            return StatusCode(statusCode ?? 400, error);
        }
        return NoContent();
    }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}

public class BlogStatusRequest
{
    public int StatusId { get; set; }
}
