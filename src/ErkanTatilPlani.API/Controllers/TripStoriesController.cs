using ErkanTatilPlani.Core.Factories.TripStories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/trip-stories")]
public class TripStoriesController : ControllerBase
{
    private readonly ITripStoryFactory _storyFactory;
    private readonly IWebHostEnvironment _env;

    public TripStoriesController(ITripStoryFactory storyFactory, IWebHostEnvironment env)
    {
        _storyFactory = storyFactory;
        _env = env;
    }

    private int? GetVisitorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetPublicFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _storyFactory.GetPublicFeedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("feed")]
    [Authorize]
    public async Task<ActionResult<object>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _storyFactory.GetFeedAsync(visitorId.Value, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetStory(int id)
    {
        var currentVisitorId = GetVisitorId();
        var result = await _storyFactory.GetStoryAsync(id, currentVisitorId);
        if (result == null) return NotFound(new { error = "Error.StoryNotFound" });
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<object>> CreateStory([FromBody] CreateStoryRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Error.TitleRequired" });
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Error.ContentRequired" });

        var result = await _storyFactory.CreateStoryAsync(
            visitorId.Value, request.Title, request.Content,
            request.TourId, request.Rating, request.TravelDate,
            request.Destination, request.IsPublic);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateStory(int id, [FromBody] CreateStoryRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _storyFactory.UpdateStoryAsync(
            visitorId.Value, id, request.Title, request.Content,
            request.TourId, request.Rating, request.TravelDate,
            request.Destination, request.IsPublic);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteStory(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _storyFactory.DeleteStoryAsync(visitorId.Value, id);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _storyFactory.ToggleLikeAsync(visitorId.Value, id);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<object>> GetComments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _storyFactory.GetCommentsAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpPost("{id}/comments")]
    [Authorize]
    public async Task<ActionResult<object>> AddComment(int id, [FromBody] StoryCommentRequest request)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Error.ContentRequired" });

        var result = await _storyFactory.AddCommentAsync(visitorId.Value, id, request.Content, request.ParentCommentId);
        return Ok(result);
    }

    [HttpDelete("comments/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _storyFactory.DeleteCommentAsync(visitorId.Value, id);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }

    [HttpPost("{id}/photos")]
    [Authorize]
    public async Task<ActionResult<object>> UploadPhoto(int id, IFormFile file, [FromForm] string? caption)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Error.FileRequired" });

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest(new { error = "Error.OnlyImages" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "Error.FileTooLarge" });

        // Dosyayi kaydet
        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "stories");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var photoUrl = $"/uploads/stories/{fileName}";
        var result = await _storyFactory.UploadPhotoAsync(visitorId.Value, id, photoUrl, caption);
        return Ok(result);
    }

    [HttpDelete("photos/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var visitorId = GetVisitorId();
        if (visitorId == null) return Unauthorized();

        var result = await _storyFactory.DeletePhotoAsync(visitorId.Value, id);
        return result.success ? Ok(new { message = result.message }) : BadRequest(new { error = result.message });
    }
}

public class CreateStoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? TourId { get; set; }
    public int? Rating { get; set; }
    public DateTime? TravelDate { get; set; }
    public string? Destination { get; set; }
    public bool IsPublic { get; set; } = true;
}

public class StoryCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}
