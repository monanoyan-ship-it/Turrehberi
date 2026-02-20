using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IGuideEntityService
{
    IQueryable<Guide> GetByCompanyId(int companyId);
    Task<Guide?> GetByIdAsync(int id);
    Task<Guide?> GetByIdWithAssignmentsAsync(int id);
    void Add(Guide guide);
    void Update(Guide guide);
    IQueryable<TourGuideAssignment> GetAssignmentsByGuideId(int guideId);
    IQueryable<TourGuideAssignment> GetAssignmentsByTourDateId(int tourDateId);
    Task<TourGuideAssignment?> GetAssignmentByIdAsync(int id);
    void AddAssignment(TourGuideAssignment assignment);
    void RemoveAssignment(TourGuideAssignment assignment);
}
