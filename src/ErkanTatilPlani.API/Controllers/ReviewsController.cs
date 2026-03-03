using ErkanTatilPlani.Core.Factories.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly ITourReviewFactory _tourReview;
    private readonly IReviewInteractionFactory _reviewInteraction;
    private readonly IAdminReviewFactory _adminReview;
    private readonly IReviewSummaryFactory _reviewSummary;

    public ReviewsController(
        ITourReviewFactory tourReview,
        IReviewInteractionFactory reviewInteraction,
        IAdminReviewFactory adminReview,
        IReviewSummaryFactory reviewSummary)
    {
        _tourReview = tourReview;
        _reviewInteraction = reviewInteraction;
        _adminReview = adminReview;
        _reviewSummary = reviewSummary;
    }

    // ===============================================
    // TUR YORUMLARI
    // ===============================================

    /// <summary>
    /// Tur yorumlarini listele (sadece onaylanmis)
    /// </summary>
    [HttpGet("tours/{tourId}/reviews")]
    public async Task<ActionResult<object>> GetTourReviews(
        int tourId,
        [FromQuery] string? sort = "newest",
        [FromQuery] int? rating = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (result, errorMessage, statusCode) = await _tourReview.GetTourReviewsAsync(tourId, sort, rating, page, pageSize);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Tur yorum ozeti - AI analiz (Faz 15.2)
    /// </summary>
    [HttpGet("tours/{tourId}/reviews/summary")]
    public async Task<ActionResult<object>> GetReviewSummary(int tourId)
    {
        var result = await _reviewSummary.GetTourReviewSummaryAsync(tourId);
        return Ok(result);
    }

    /// <summary>
    /// Yeni yorum ekle
    /// </summary>
    [HttpPost("tours/{tourId}/reviews")]
    [Authorize]
    public async Task<ActionResult<object>> CreateReview(int tourId, [FromBody] CreateReviewRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var userAgent = Request.Headers.UserAgent.ToString();

        var (result, errorMessage, statusCode) = await _tourReview.CreateReviewAsync(
            tourId, request.VisitorId, request.OverallRating, request.ServiceRating,
            request.ValueRating, request.LocationRating, request.OrganizationRating,
            request.GuideRating, request.Title, request.Pros, request.Cons, request.Comment,
            request.VisitDate, request.TravelTypeId, request.WouldRecommend, ipAddress, userAgent);

        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Yorum guncelle
    /// </summary>
    [HttpPut("reviews/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
    {
        var (result, errorMessage, statusCode) = await _tourReview.UpdateReviewAsync(
            id, request.VisitorId, request.OverallRating, request.ServiceRating,
            request.ValueRating, request.LocationRating, request.OrganizationRating,
            request.GuideRating, request.Title, request.Pros, request.Cons, request.Comment,
            request.VisitDate, request.TravelTypeId, request.WouldRecommend);

        if (statusCode == 403) return Forbid();
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Yorum sil
    /// </summary>
    [HttpDelete("reviews/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id, [FromQuery] int visitorId)
    {
        var (result, errorMessage, statusCode) = await _tourReview.DeleteReviewAsync(id, visitorId);
        if (statusCode == 403) return Forbid();
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    // ===============================================
    // YARDIMCI OYLAMA
    // ===============================================

    /// <summary>
    /// Yorumu yardimci/yardimci degil olarak oyla
    /// </summary>
    [HttpPost("reviews/{id}/helpful")]
    [Authorize]
    public async Task<IActionResult> VoteHelpful(int id, [FromBody] VoteHelpfulRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var (result, errorMessage, statusCode) = await _reviewInteraction.VoteHelpfulAsync(id, request.VisitorId, request.IsHelpful, ipAddress);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    // ===============================================
    // YANITLAR
    // ===============================================

    /// <summary>
    /// Yoruma yanit ekle
    /// </summary>
    [HttpPost("reviews/{id}/reply")]
    [Authorize]
    public async Task<ActionResult<object>> AddReply(int id, [FromBody] AddReplyRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var (result, errorMessage, statusCode) = await _reviewInteraction.AddReplyAsync(id, request.VisitorId, request.ParentReplyId, request.Comment, ipAddress);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    // ===============================================
    // RESIM YUKLEME
    // ===============================================

    /// <summary>
    /// Yoruma resim yukle
    /// </summary>
    [HttpPost("reviews/{id}/images")]
    [Authorize]
    public async Task<ActionResult<object>> UploadReviewImage(int id, [FromForm] IFormFile file, [FromForm] int visitorId, [FromForm] string? caption = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Error.NoFileSelected" });

        using var stream = file.OpenReadStream();
        var (result, errorMessage, statusCode) = await _reviewInteraction.UploadReviewImageAsync(id, visitorId, stream, file.FileName, caption);
        if (statusCode == 403) return Forbid();
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Yorum resimlerini listele
    /// </summary>
    [HttpGet("reviews/{id}/images")]
    public async Task<ActionResult<object>> GetReviewImages(int id)
    {
        var (result, errorMessage, statusCode) = await _reviewInteraction.GetReviewImagesAsync(id);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Yorum resmini sil
    /// </summary>
    [HttpDelete("reviews/{reviewId}/images/{imageId}")]
    [Authorize]
    public async Task<IActionResult> DeleteReviewImage(int reviewId, int imageId, [FromQuery] int visitorId)
    {
        var (result, errorMessage, statusCode) = await _reviewInteraction.DeleteReviewImageAsync(reviewId, imageId, visitorId);
        if (statusCode == 403) return Forbid();
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    // ===============================================
    // SIKAYET
    // ===============================================

    /// <summary>
    /// Yorum sikayet et
    /// </summary>
    [HttpPost("reviews/{id}/report")]
    [Authorize]
    public async Task<IActionResult> ReportReview(int id, [FromBody] ReportRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var (result, errorMessage, statusCode) = await _reviewInteraction.ReportReviewAsync(id, request.VisitorId, request.ReasonId, request.Description, ipAddress);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    // ===============================================
    // FIRMA SAHIBI YORUMLARI
    // ===============================================

    /// <summary>
    /// Firma sahibinin turlarina yapilan yorumlari listele
    /// </summary>
    [HttpGet("reviews/my")]
    [Authorize(Roles = "CompanyOwner,Staff,Admin")]
    public async Task<ActionResult<object>> GetMyReviews([FromQuery] int companyId, [FromQuery] bool? hasReply = null)
    {
        return Ok(await _tourReview.GetMyReviewsAsync(companyId, hasReply));
    }

    // ===============================================
    // ADMIN MODERASYON
    // ===============================================

    /// <summary>
    /// Tum yorumlari listele (Admin)
    /// </summary>
    [HttpGet("admin/reviews")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<object>> GetAllReviews(
        [FromQuery] int? statusId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _adminReview.GetAllReviewsAsync(statusId, search, page, pageSize));
    }

    /// <summary>
    /// Yorum onayla (Admin)
    /// </summary>
    [HttpPost("reviews/{id}/approve")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ApproveReview(int id, [FromBody] ModerateReviewRequest request)
    {
        var (result, errorMessage, statusCode) = await _adminReview.ApproveReviewAsync(id, request.ModeratedById, request.Note);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Yorum reddet (Admin)
    /// </summary>
    [HttpPost("reviews/{id}/reject")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> RejectReview(int id, [FromBody] RejectReviewRequest request)
    {
        var (result, errorMessage, statusCode) = await _adminReview.RejectReviewAsync(id, request.ModeratedById, request.RejectionReason, request.Note);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Yorumu incelemeye al (Flagged durumuna getir)
    /// </summary>
    [HttpPost("reviews/{id}/flag")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> FlagReview(int id, [FromBody] ModerateReviewRequest request)
    {
        var (result, errorMessage, statusCode) = await _adminReview.FlagReviewAsync(id, request.ModeratedById, request.Note);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }

    /// <summary>
    /// Sikayet raporlarini listele (Admin)
    /// </summary>
    [HttpGet("admin/reports")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<object>> GetReports([FromQuery] int? statusId = null)
    {
        return Ok(await _adminReview.GetReportsAsync(statusId));
    }

    /// <summary>
    /// Sikayet coz (Admin)
    /// </summary>
    [HttpPost("reports/{id}/resolve")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportRequest request)
    {
        var (result, errorMessage, statusCode) = await _adminReview.ResolveReportAsync(id, request.ReviewedById, request.RemoveReview, request.Note);
        if (errorMessage != null) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(result);
    }
}

// ===============================================
// REQUEST DTO'LARI
// ===============================================

public class CreateReviewRequest
{
    public int VisitorId { get; set; }
    public int OverallRating { get; set; }
    public int? ServiceRating { get; set; }
    public int? ValueRating { get; set; }
    public int? LocationRating { get; set; }
    public int? OrganizationRating { get; set; }
    public int? GuideRating { get; set; }
    public string? Title { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public string? Comment { get; set; }
    public DateTime? VisitDate { get; set; }
    public int TravelTypeId { get; set; }
    public bool WouldRecommend { get; set; } = true;
}

public class UpdateReviewRequest : CreateReviewRequest { }

public class VoteHelpfulRequest
{
    public int VisitorId { get; set; }
    public bool IsHelpful { get; set; }
}

public class AddReplyRequest
{
    public int VisitorId { get; set; }
    public int? ParentReplyId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ReportRequest
{
    public int VisitorId { get; set; }
    public int ReasonId { get; set; }
    public string? Description { get; set; }
}

public class ModerateReviewRequest
{
    public int? ModeratedById { get; set; }
    public string? Note { get; set; }
}

public class RejectReviewRequest
{
    public int? ModeratedById { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class ResolveReportRequest
{
    public int? ReviewedById { get; set; }
    public bool RemoveReview { get; set; }
    public string? Note { get; set; }
}
