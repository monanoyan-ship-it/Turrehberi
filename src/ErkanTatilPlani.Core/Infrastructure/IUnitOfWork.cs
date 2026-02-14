namespace ErkanTatilPlani.Core.Infrastructure;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
