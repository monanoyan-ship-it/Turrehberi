using System.Security.Claims;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Infrastructure;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public int? GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    public async Task<Visitor?> GetCurrentVisitorAsync()
    {
        var userId = GetUserId();
        if (userId == null) return null;
        return await _context.Visitors.FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
    }

    public async Task<Visitor?> GetCurrentVisitorWithCompanyAsync()
    {
        var userId = GetUserId();
        if (userId == null) return null;
        return await _context.Visitors
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == userId && v.IsActive);
    }
}
