namespace ErkanTatilPlani.Core.Factories.Referrals;

public interface IReferralFactory
{
    Task<string> GetOrCreateReferralCodeAsync(int visitorId);
    Task ProcessReferralSignupAsync(int newVisitorId, string referralCode);
    Task ProcessReferralReservationAsync(int visitorId, int reservationId);
    Task<object> GetReferralDashboardAsync(int visitorId);
}
