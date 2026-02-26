using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Referrals;
using ErkanTatilPlani.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Referrals;

public class ReferralFactory : IReferralFactory
{
    private readonly IReferralEntityService _referralService;
    private readonly IVisitorEntityService _visitorService;
    private readonly ILoyaltyEntityService _loyaltyEntityService;
    private readonly IUnitOfWork _unitOfWork;

    public ReferralFactory(
        IReferralEntityService referralService,
        IVisitorEntityService visitorService,
        ILoyaltyEntityService loyaltyEntityService,
        IUnitOfWork unitOfWork)
    {
        _referralService = referralService;
        _visitorService = visitorService;
        _loyaltyEntityService = loyaltyEntityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GetOrCreateReferralCodeAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(visitor.ReferralCode))
            return visitor.ReferralCode;

        var code = GenerateReferralCode();
        visitor.ReferralCode = code;
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);
        await _unitOfWork.SaveChangesAsync();

        return code;
    }

    public async Task ProcessReferralSignupAsync(int newVisitorId, string referralCode)
    {
        if (string.IsNullOrWhiteSpace(referralCode)) return;

        var referrer = await _visitorService.GetByReferralCodeAsync(referralCode);
        if (referrer == null || referrer.Id == newVisitorId) return;

        var existing = await _referralService.GetByReferredVisitorIdAsync(newVisitorId);
        if (existing != null) return;

        _referralService.Add(new Referral
        {
            ReferrerVisitorId = referrer.Id,
            ReferredVisitorId = newVisitorId,
            ReferralCode = referralCode,
            StatusId = ReferralStatuses.Ids.Pending,
            BonusAmount = 50
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ProcessReferralReservationAsync(int visitorId, int reservationId)
    {
        var referral = await _referralService.GetByReferredVisitorIdAsync(visitorId);
        if (referral == null || referral.StatusId != ReferralStatuses.Ids.Pending) return;

        referral.StatusId = ReferralStatuses.Ids.Completed;
        referral.ReservationId = reservationId;
        referral.CompletedAt = DateTime.UtcNow;
        _referralService.Update(referral);
        await _unitOfWork.SaveChangesAsync();

        var bonusAmount = referral.BonusAmount;
        await GiveReferralBonusAsync(referral.ReferrerVisitorId, bonusAmount, reservationId, "Referral bonus - friend made first reservation");
        await GiveReferralBonusAsync(referral.ReferredVisitorId, bonusAmount, reservationId, "Referral bonus - first reservation with referral code");

        referral.StatusId = ReferralStatuses.Ids.Rewarded;
        referral.RewardedAt = DateTime.UtcNow;
        _referralService.Update(referral);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<object> GetReferralDashboardAsync(int visitorId)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null)
            return new { error = "Error.VisitorNotFound" };

        var code = await GetOrCreateReferralCodeAsync(visitorId);

        var referrals = await _referralService.GetByReferrerId(visitorId)
            .Include(r => r.ReferredVisitor)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                referredName = r.ReferredVisitor.FirstName + " " + r.ReferredVisitor.LastName,
                statusId = r.StatusId,
                statusName = ReferralStatuses.GetById(r.StatusId)!.SystemName,
                statusNameKey = ReferralStatuses.GetById(r.StatusId)!.NameResourceKey,
                r.BonusAmount,
                r.CreatedAt,
                r.CompletedAt,
                r.RewardedAt
            })
            .ToListAsync();

        var totalEarned = referrals.Where(r => r.statusId == ReferralStatuses.Ids.Rewarded).Sum(r => r.BonusAmount);

        return new
        {
            referralCode = code,
            totalReferrals = referrals.Count,
            completedReferrals = referrals.Count(r => r.statusId >= ReferralStatuses.Ids.Completed),
            totalEarned,
            referrals
        };
    }

    private async Task GiveReferralBonusAsync(int visitorId, decimal amount, int reservationId, string description)
    {
        var visitor = await _visitorService.GetByIdAsync(visitorId);
        if (visitor == null) return;

        visitor.CreditBalance += amount;
        visitor.UpdatedAt = DateTime.UtcNow;
        _visitorService.Update(visitor);

        _loyaltyEntityService.AddCredit(new TourCredit
        {
            VisitorId = visitorId,
            TransactionTypeId = CreditTransactionTypes.Ids.ReferralBonus,
            Amount = amount,
            BalanceAfter = visitor.CreditBalance,
            ReservationId = reservationId,
            Description = description,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        });

        await _unitOfWork.SaveChangesAsync();
    }

    private static string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
