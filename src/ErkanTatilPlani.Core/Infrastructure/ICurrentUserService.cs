using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Infrastructure;

public interface ICurrentUserService
{
    int? GetUserId();
    Task<Visitor?> GetCurrentVisitorAsync();
    Task<Visitor?> GetCurrentVisitorWithCompanyAsync();
}
