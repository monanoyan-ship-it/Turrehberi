using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class GuideEntityService : IGuideEntityService
{
    private readonly AppDbContext _context;

    public GuideEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Guide> GetByCompanyId(int companyId)
        => _context.Guides.Where(g => g.CompanyId == companyId && g.IsActive);

    public async Task<Guide?> GetByIdAsync(int id)
        => await _context.Guides.FindAsync(id);

    public async Task<Guide?> GetByIdWithAssignmentsAsync(int id)
        => await _context.Guides
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.TourDate)
                    .ThenInclude(td => td.Tour)
            .FirstOrDefaultAsync(g => g.Id == id);

    public void Add(Guide guide) => _context.Guides.Add(guide);

    public void Update(Guide guide) => _context.Entry(guide).State = EntityState.Modified;

    public IQueryable<TourGuideAssignment> GetAssignmentsByGuideId(int guideId)
        => _context.TourGuideAssignments
            .Include(a => a.TourDate).ThenInclude(td => td.Tour)
            .Where(a => a.GuideId == guideId && a.IsActive);

    public IQueryable<TourGuideAssignment> GetAssignmentsByTourDateId(int tourDateId)
        => _context.TourGuideAssignments
            .Include(a => a.Guide)
            .Where(a => a.TourDateId == tourDateId && a.IsActive);

    public async Task<TourGuideAssignment?> GetAssignmentByIdAsync(int id)
        => await _context.TourGuideAssignments
            .Include(a => a.Guide)
            .Include(a => a.TourDate)
            .FirstOrDefaultAsync(a => a.Id == id);

    public void AddAssignment(TourGuideAssignment assignment)
        => _context.TourGuideAssignments.Add(assignment);

    public void RemoveAssignment(TourGuideAssignment assignment)
    {
        assignment.IsActive = false;
        assignment.UpdatedAt = DateTime.UtcNow;
    }
}
