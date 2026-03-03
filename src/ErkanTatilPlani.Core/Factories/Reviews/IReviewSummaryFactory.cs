namespace ErkanTatilPlani.Core.Factories.Reviews;

/// <summary>
/// AI yorum ozeti - yorumlardan ortak temalari cikarir (Faz 15.2)
/// </summary>
public interface IReviewSummaryFactory
{
    Task<object> GetTourReviewSummaryAsync(int tourId);
}
