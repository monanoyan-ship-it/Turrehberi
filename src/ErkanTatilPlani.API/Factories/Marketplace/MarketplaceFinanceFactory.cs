using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Core.Enums;
using ErkanTatilPlani.Core.Factories.Marketplace;
using ErkanTatilPlani.Core.Infrastructure;
using ErkanTatilPlani.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.Factories.Marketplace;

public class MarketplaceFinanceFactory : IMarketplaceFinanceFactory
{
    private readonly IMarketplaceFinanceEntityService _finance;
    private readonly ICompanyEntityService _companies;
    private readonly IVisitorEntityService _visitors;
    private readonly IMarketplacePaymentService _marketplacePayment;
    private readonly IUnitOfWork _unitOfWork;

    public MarketplaceFinanceFactory(
        IMarketplaceFinanceEntityService finance,
        ICompanyEntityService companies,
        IVisitorEntityService visitors,
        IMarketplacePaymentService marketplacePayment,
        IUnitOfWork unitOfWork)
    {
        _finance = finance;
        _companies = companies;
        _visitors = visitors;
        _marketplacePayment = marketplacePayment;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetAdminOverviewAsync()
    {
        var paidTransactions = _finance.GetTransactions()
            .Where(t => t.StatusId == MarketplaceTransactionStatuses.Ids.Paid
                || t.StatusId == MarketplaceTransactionStatuses.Ids.PartiallyRefunded
                || t.StatusId == MarketplaceTransactionStatuses.Ids.Refunded);

        var sellerLedger = _finance.GetLedgerEntries()
            .Where(e => e.EntryTypeId == LedgerEntryTypes.Ids.SellerReceivable);

        var summary = new
        {
            grossVolume = await paidTransactions.SumAsync(t => (decimal?)t.PaidAmount) ?? 0,
            platformCommission = await paidTransactions.SumAsync(t => (decimal?)t.PlatformCommissionAmount) ?? 0,
            sellerReceivable = await sellerLedger.SumAsync(e => (decimal?)e.Amount) ?? 0,
            availableForPayout = await sellerLedger
                .Where(e => e.StatusId != LedgerEntryStatuses.Ids.Settled && e.AvailableAt <= DateTime.UtcNow)
                .SumAsync(e => (decimal?)e.Amount) ?? 0,
            refundedAmount = await _finance.GetRefunds()
                .Where(r => r.StatusId == MarketplaceRefundStatuses.Ids.Processed)
                .SumAsync(r => (decimal?)r.Amount) ?? 0,
            activeSellers = await _companies.GetActiveApprovedCompanies()
                .Where(c => c.MarketplaceEnabled && c.SellerOnboardingStatusId == SellerOnboardingStatuses.Ids.Active)
                .CountAsync(),
            sellersWaitingOnboarding = await _companies.GetActiveApprovedCompanies()
                .Where(c => c.SellerOnboardingStatusId != SellerOnboardingStatuses.Ids.Active)
                .CountAsync()
        };

        var recentTransactions = await BuildTransactionsQuery(null, null)
            .OrderByDescending(t => t.CreatedAt)
            .Take(8)
            .ToListAsync();

        var payouts = await BuildPayoutsQuery(null, null)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        return new
        {
            summary,
            recentTransactions = recentTransactions.Select(MapTransaction),
            recentPayouts = payouts.Select(MapPayout)
        };
    }

    public async Task<object> GetAdminSellersAsync()
    {
        var companies = await _companies.GetActiveApprovedCompanies()
            .OrderBy(c => c.Name)
            .ToListAsync();
        var companyIds = companies.Select(c => c.Id).ToList();
        var paidByCompany = await _finance.GetTransactions()
            .Where(t => companyIds.Contains(t.CompanyId) && t.StatusId == MarketplaceTransactionStatuses.Ids.Paid)
            .GroupBy(t => t.CompanyId)
            .Select(g => new
            {
                companyId = g.Key,
                gross = g.Sum(t => t.PaidAmount),
                commission = g.Sum(t => t.PlatformCommissionAmount),
                seller = g.Sum(t => t.SellerReceivableAmount)
            })
            .ToDictionaryAsync(x => x.companyId);

        return new
        {
            sellers = companies.Select(c =>
            {
                paidByCompany.TryGetValue(c.Id, out var finance);
                return new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Phone,
                    c.TaxNumber,
                    c.MarketplaceEnabled,
                    c.SellerLegalTypeId,
                    sellerLegalType = SellerLegalTypes.GetById(c.SellerLegalTypeId)?.SystemName,
                    c.SellerOnboardingStatusId,
                    onboardingStatus = SellerOnboardingStatuses.GetById(c.SellerOnboardingStatusId)?.SystemName,
                    c.PlatformCommissionRate,
                    c.PayoutDelayDays,
                    c.LegalCompanyTitle,
                    c.TaxOffice,
                    c.Iban,
                    c.ContactName,
                    c.ContactSurname,
                    c.SubMerchantExternalId,
                    hasSubMerchantKey = !string.IsNullOrWhiteSpace(c.SubMerchantKey),
                    c.OnboardingErrorCode,
                    c.OnboardingErrorMessage,
                    c.OnboardedAt,
                    grossVolume = finance?.gross ?? 0,
                    platformCommission = finance?.commission ?? 0,
                    sellerReceivable = finance?.seller ?? 0,
                    readyForOnboarding = IsSellerSettingsComplete(c)
                };
            })
        };
    }

    public async Task<object> GetAdminTransactionsAsync(int? companyId = null, int? statusId = null)
    {
        var transactions = await BuildTransactionsQuery(companyId, statusId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(250)
            .ToListAsync();

        return new { transactions = transactions.Select(MapTransaction) };
    }

    public async Task<object> GetAdminRefundsAsync(int? companyId = null, int? statusId = null)
    {
        var refunds = await BuildRefundsQuery(companyId, statusId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(250)
            .ToListAsync();

        return new { refunds = refunds.Select(MapRefund) };
    }

    public async Task<object> GetAdminPayoutsAsync(int? companyId = null, int? statusId = null)
    {
        var payouts = await BuildPayoutsQuery(companyId, statusId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(250)
            .ToListAsync();

        return new { payouts = payouts.Select(MapPayout) };
    }

    public async Task<(bool success, object result, int statusCode)> UpdateSellerSettingsAsync(int companyId, MarketplaceSellerSettingsRequest request)
    {
        var company = await _companies.GetByIdAsync(companyId);
        if (company == null || !company.IsActive)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        ApplySellerSettings(company, request);
        _companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Marketplace.SellerSettingsSaved", seller = MapSeller(company) }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> OnboardSellerAsync(int companyId)
    {
        var company = await _companies.GetByIdAsync(companyId);
        if (company == null || !company.IsActive)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        if (!IsSellerSettingsComplete(company))
            return (false, new { message = "Marketplace.SellerInfoIncomplete" }, 400);

        company.SellerOnboardingStatusId = SellerOnboardingStatuses.Ids.Submitted;
        company.OnboardingErrorCode = string.Empty;
        company.OnboardingErrorMessage = string.Empty;
        _companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        var providerResult = await _marketplacePayment.CreateOrUpdateSubMerchantAsync(new SellerOnboardingRequest
        {
            CompanyId = company.Id,
            ExternalId = EnsureExternalId(company),
            SellerLegalTypeId = company.SellerLegalTypeId,
            Name = company.Name,
            LegalCompanyTitle = company.LegalCompanyTitle,
            TaxOffice = company.TaxOffice,
            TaxNumber = company.TaxNumber,
            Iban = company.Iban,
            Address = company.Address,
            ContactName = company.ContactName,
            ContactSurname = company.ContactSurname,
            Email = company.Email,
            Phone = company.Phone,
            HasExistingSubMerchantKey = !string.IsNullOrWhiteSpace(company.SubMerchantKey),
            SubMerchantKey = company.SubMerchantKey
        });

        if (providerResult.Success)
        {
            company.SubMerchantKey = providerResult.SubMerchantKey ?? company.SubMerchantKey;
            company.MarketplaceEnabled = true;
            company.SellerOnboardingStatusId = SellerOnboardingStatuses.Ids.Active;
            company.OnboardedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;
            _companies.Update(company);
            await _unitOfWork.SaveChangesAsync();
            return (true, new { message = "Marketplace.SellerOnboarded", seller = MapSeller(company) }, 200);
        }

        company.SellerOnboardingStatusId = SellerOnboardingStatuses.Ids.Failed;
        company.OnboardingErrorCode = providerResult.ErrorCode ?? string.Empty;
        company.OnboardingErrorMessage = providerResult.ErrorMessage ?? "Alt uye kaydi basarisiz";
        company.UpdatedAt = DateTime.UtcNow;
        _companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        return (false, new
        {
            message = "Marketplace.SellerOnboardingFailed",
            errorCode = providerResult.ErrorCode,
            errorMessage = providerResult.ErrorMessage
        }, 400);
    }

    public async Task<(bool success, object result, int statusCode)> CreateRefundAsync(int transactionId, CreateMarketplaceRefundRequest request, int? processedById)
    {
        var transaction = await _finance.GetTransactionByIdAsync(transactionId);
        if (transaction == null)
            return (false, new { message = "Marketplace.TransactionNotFound" }, 404);

        if (request.Amount <= 0)
            return (false, new { message = "Marketplace.RefundAmountInvalid" }, 400);

        var refundableAmount = transaction.PaidAmount - transaction.RefundedAmount;
        if (request.Amount > refundableAmount)
            return (false, new { message = "Marketplace.RefundAmountTooHigh", refundableAmount }, 400);

        var lineItem = transaction.LineItems.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.ProviderPaymentTransactionId));
        var refund = new MarketplaceRefund
        {
            PaymentTransactionId = transaction.Id,
            ReservationId = transaction.ReservationId,
            CompanyId = transaction.CompanyId,
            StatusId = MarketplaceRefundStatuses.Ids.Requested,
            RequestedById = processedById,
            ProcessedById = processedById,
            Amount = request.Amount,
            Reason = request.Reason,
            ProviderPaymentTransactionId = lineItem?.ProviderPaymentTransactionId
        };
        _finance.AddRefund(refund);
        await _unitOfWork.SaveChangesAsync();

        MarketplaceProviderRefundResult providerResult;
        if (!string.IsNullOrWhiteSpace(lineItem?.ProviderPaymentTransactionId))
        {
            providerResult = await _marketplacePayment.RefundAsync(new MarketplaceProviderRefundRequest
            {
                PaymentTransactionId = lineItem.ProviderPaymentTransactionId!,
                Amount = request.Amount,
                ConversationId = $"REF-{refund.Id}-{DateTime.UtcNow.Ticks}"
            });
        }
        else
        {
            providerResult = new MarketplaceProviderRefundResult
            {
                Success = true,
                RefundId = $"MANUAL-{refund.Id}"
            };
        }

        if (!providerResult.Success)
        {
            refund.StatusId = MarketplaceRefundStatuses.Ids.Failed;
            refund.ErrorCode = providerResult.ErrorCode;
            refund.ErrorMessage = providerResult.ErrorMessage;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return (false, new { message = "Marketplace.RefundFailed", providerResult.ErrorCode, providerResult.ErrorMessage }, 400);
        }

        refund.StatusId = MarketplaceRefundStatuses.Ids.Processed;
        refund.ProviderRefundId = providerResult.RefundId;
        refund.ProcessedAt = DateTime.UtcNow;
        refund.UpdatedAt = DateTime.UtcNow;

        transaction.RefundedAmount += request.Amount;
        transaction.StatusId = transaction.RefundedAmount >= transaction.PaidAmount
            ? MarketplaceTransactionStatuses.Ids.Refunded
            : MarketplaceTransactionStatuses.Ids.PartiallyRefunded;
        transaction.RefundedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        if (transaction.RefundedAmount >= transaction.PaidAmount)
        {
            transaction.Reservation.PaymentStatus = PaymentStatuses.Ids.Refunded;
            transaction.Reservation.UpdatedAt = DateTime.UtcNow;
        }

        _finance.AddLedgerEntry(new MarketplaceLedgerEntry
        {
            PaymentTransactionId = transaction.Id,
            ReservationId = transaction.ReservationId,
            CompanyId = transaction.CompanyId,
            EntryTypeId = LedgerEntryTypes.Ids.Refund,
            StatusId = LedgerEntryStatuses.Ids.Settled,
            Amount = -request.Amount,
            Reference = providerResult.RefundId ?? $"REF-{refund.Id}",
            Description = string.IsNullOrWhiteSpace(request.Reason) ? "Iade" : request.Reason,
            OccurredAt = DateTime.UtcNow,
            SettledAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return (true, new { message = "Marketplace.RefundProcessed", refund = MapRefund(refund) }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> CreatePayoutBatchAsync(int companyId, CreatePayoutBatchRequest request, int? approvedById)
    {
        var company = await _companies.GetByIdAsync(companyId);
        if (company == null || !company.IsActive)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        var periodStart = request.PeriodStart ?? DateTime.UtcNow.AddMonths(-1);
        var periodEnd = request.PeriodEnd ?? DateTime.UtcNow;
        var eligibleEntries = await _finance.GetLedgerEntries()
            .Where(e => e.CompanyId == companyId
                && e.EntryTypeId == LedgerEntryTypes.Ids.SellerReceivable
                && e.StatusId != LedgerEntryStatuses.Ids.Settled
                && e.AvailableAt <= periodEnd)
            .OrderBy(e => e.AvailableAt)
            .ToListAsync();

        if (eligibleEntries.Count == 0)
            return (false, new { message = "Marketplace.NoPayoutEntries" }, 400);

        var refundAmount = await _finance.GetLedgerEntries()
            .Where(e => e.CompanyId == companyId
                && e.EntryTypeId == LedgerEntryTypes.Ids.Refund
                && e.OccurredAt >= periodStart
                && e.OccurredAt <= periodEnd)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var grossAmount = eligibleEntries.Sum(e => e.Amount);
        var netAmount = Math.Max(grossAmount + refundAmount, 0);

        var payout = new PayoutBatch
        {
            CompanyId = companyId,
            StatusId = PayoutStatuses.Ids.Approved,
            ApprovedById = approvedById,
            BatchNumber = $"PO-{companyId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GrossAmount = grossAmount,
            PlatformCommissionAmount = await _finance.GetTransactions()
                .Where(t => t.CompanyId == companyId && t.PaidAt >= periodStart && t.PaidAt <= periodEnd)
                .SumAsync(t => (decimal?)t.PlatformCommissionAmount) ?? 0,
            RefundAmount = Math.Abs(refundAmount),
            NetAmount = netAmount,
            Notes = request.Notes,
            ApprovedAt = DateTime.UtcNow
        };

        _finance.AddPayoutBatch(payout);
        await _unitOfWork.SaveChangesAsync();

        foreach (var entry in eligibleEntries)
        {
            entry.StatusId = LedgerEntryStatuses.Ids.Settled;
            entry.PayoutBatchId = payout.Id;
            entry.SettledAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
        }

        _finance.AddLedgerEntry(new MarketplaceLedgerEntry
        {
            CompanyId = companyId,
            PayoutBatchId = payout.Id,
            EntryTypeId = LedgerEntryTypes.Ids.Payout,
            StatusId = LedgerEntryStatuses.Ids.Settled,
            Amount = -netAmount,
            Reference = payout.BatchNumber,
            Description = "Hak edis odemesi",
            OccurredAt = DateTime.UtcNow,
            SettledAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return (true, new { message = "Marketplace.PayoutCreated", payout = MapPayout(payout) }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> MarkPayoutPaidAsync(int payoutId, MarkPayoutPaidRequest request, int? approvedById)
    {
        var payout = await _finance.GetPayoutBatchByIdAsync(payoutId);
        if (payout == null)
            return (false, new { message = "Marketplace.PayoutNotFound" }, 404);

        payout.StatusId = PayoutStatuses.Ids.Paid;
        payout.BankReference = request.BankReference;
        payout.Notes = string.IsNullOrWhiteSpace(request.Notes) ? payout.Notes : request.Notes;
        payout.ApprovedById ??= approvedById;
        payout.PaidAt = DateTime.UtcNow;
        payout.UpdatedAt = DateTime.UtcNow;
        _finance.UpdatePayoutBatch(payout);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Marketplace.PayoutPaid", payout = MapPayout(payout) }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> GetCompanyOverviewAsync(int visitorId)
    {
        var company = await GetCompanyForVisitorAsync(visitorId);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        var transactions = await BuildTransactionsQuery(company.Id, null)
            .OrderByDescending(t => t.CreatedAt)
            .Take(100)
            .ToListAsync();
        var payouts = await BuildPayoutsQuery(company.Id, null)
            .OrderByDescending(p => p.CreatedAt)
            .Take(100)
            .ToListAsync();
        var refunds = await BuildRefundsQuery(company.Id, null)
            .OrderByDescending(r => r.CreatedAt)
            .Take(100)
            .ToListAsync();

        var sellerLedger = _finance.GetLedgerEntries()
            .Where(e => e.CompanyId == company.Id && e.EntryTypeId == LedgerEntryTypes.Ids.SellerReceivable);

        return (true, new
        {
            seller = MapSeller(company),
            summary = new
            {
                grossVolume = transactions.Sum(t => t.PaidAmount),
                sellerReceivable = transactions.Sum(t => t.SellerReceivableAmount),
                availableForPayout = await sellerLedger
                    .Where(e => e.StatusId != LedgerEntryStatuses.Ids.Settled && e.AvailableAt <= DateTime.UtcNow)
                    .SumAsync(e => (decimal?)e.Amount) ?? 0,
                pendingPayout = await sellerLedger
                    .Where(e => e.StatusId != LedgerEntryStatuses.Ids.Settled && e.AvailableAt > DateTime.UtcNow)
                    .SumAsync(e => (decimal?)e.Amount) ?? 0,
                refundedAmount = refunds.Where(r => r.StatusId == MarketplaceRefundStatuses.Ids.Processed).Sum(r => r.Amount)
            },
            transactions = transactions.Select(MapTransaction),
            payouts = payouts.Select(MapPayout),
            refunds = refunds.Select(MapRefund)
        }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> UpdateMySellerSettingsAsync(int visitorId, MarketplaceSellerSettingsRequest request)
    {
        var company = await GetCompanyForVisitorAsync(visitorId);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        ApplySellerSettings(company, request, allowCommissionUpdate: false);
        _companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "Marketplace.SellerSettingsSaved", seller = MapSeller(company) }, 200);
    }

    public async Task<(bool success, object result, int statusCode)> OnboardMySellerAsync(int visitorId)
    {
        var company = await GetCompanyForVisitorAsync(visitorId);
        if (company == null)
            return (false, new { message = "Error.CompanyNotFound" }, 404);

        return await OnboardSellerAsync(company.Id);
    }

    private IQueryable<PaymentTransaction> BuildTransactionsQuery(int? companyId, int? statusId)
    {
        var query = _finance.GetTransactions()
            .Include(t => t.Company)
            .Include(t => t.Reservation).ThenInclude(r => r.Tour)
            .Include(t => t.Visitor)
            .Include(t => t.LineItems)
            .AsQueryable();

        if (companyId.HasValue) query = query.Where(t => t.CompanyId == companyId.Value);
        if (statusId.HasValue) query = query.Where(t => t.StatusId == statusId.Value);
        return query;
    }

    private IQueryable<MarketplaceRefund> BuildRefundsQuery(int? companyId, int? statusId)
    {
        var query = _finance.GetRefunds()
            .Include(r => r.Company)
            .Include(r => r.Reservation).ThenInclude(res => res.Tour)
            .Include(r => r.PaymentTransaction)
            .AsQueryable();

        if (companyId.HasValue) query = query.Where(r => r.CompanyId == companyId.Value);
        if (statusId.HasValue) query = query.Where(r => r.StatusId == statusId.Value);
        return query;
    }

    private IQueryable<PayoutBatch> BuildPayoutsQuery(int? companyId, int? statusId)
    {
        var query = _finance.GetPayoutBatches()
            .Include(p => p.Company)
            .Include(p => p.LedgerEntries)
            .AsQueryable();

        if (companyId.HasValue) query = query.Where(p => p.CompanyId == companyId.Value);
        if (statusId.HasValue) query = query.Where(p => p.StatusId == statusId.Value);
        return query;
    }

    private async Task<Company?> GetCompanyForVisitorAsync(int visitorId)
    {
        var visitor = await _visitors.GetByIdWithCompanyAsync(visitorId);
        return visitor?.Company;
    }

    private void ApplySellerSettings(Company company, MarketplaceSellerSettingsRequest request, bool allowCommissionUpdate = true)
    {
        company.SellerLegalTypeId = request.SellerLegalTypeId > 0
            ? request.SellerLegalTypeId
            : company.SellerLegalTypeId;
        company.MarketplaceEnabled = request.MarketplaceEnabled;
        if (allowCommissionUpdate)
        {
            company.PlatformCommissionRate = request.PlatformCommissionRate is >= 0 and <= 80
                ? request.PlatformCommissionRate
                : company.PlatformCommissionRate;
            company.PayoutDelayDays = request.PayoutDelayDays is >= 0 and <= 90
                ? request.PayoutDelayDays
                : company.PayoutDelayDays;
        }

        company.LegalCompanyTitle = request.LegalCompanyTitle.Trim();
        company.TaxOffice = request.TaxOffice.Trim();
        company.TaxNumber = string.IsNullOrWhiteSpace(request.TaxNumber) ? company.TaxNumber : request.TaxNumber.Trim();
        company.Iban = NormalizeIban(request.Iban);
        company.ContactName = request.ContactName.Trim();
        company.ContactSurname = request.ContactSurname.Trim();
        company.Email = string.IsNullOrWhiteSpace(request.Email) ? company.Email : request.Email.Trim();
        company.Phone = string.IsNullOrWhiteSpace(request.Phone) ? company.Phone : request.Phone.Trim();
        company.Address = string.IsNullOrWhiteSpace(request.Address) ? company.Address : request.Address.Trim();
        company.SubMerchantExternalId = EnsureExternalId(company);
        company.SellerOnboardingStatusId = IsSellerSettingsComplete(company)
            ? SellerOnboardingStatuses.Ids.ReadyForSubmission
            : SellerOnboardingStatuses.Ids.MissingInfo;
        if (!string.IsNullOrWhiteSpace(company.SubMerchantKey))
        {
            company.SellerOnboardingStatusId = SellerOnboardingStatuses.Ids.Active;
        }
        company.UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsSellerSettingsComplete(Company company)
        => !string.IsNullOrWhiteSpace(company.Name)
            && !string.IsNullOrWhiteSpace(company.Email)
            && !string.IsNullOrWhiteSpace(company.Phone)
            && !string.IsNullOrWhiteSpace(company.Address)
            && !string.IsNullOrWhiteSpace(company.TaxNumber)
            && !string.IsNullOrWhiteSpace(company.LegalCompanyTitle)
            && !string.IsNullOrWhiteSpace(company.TaxOffice)
            && !string.IsNullOrWhiteSpace(company.Iban)
            && !string.IsNullOrWhiteSpace(company.ContactName)
            && !string.IsNullOrWhiteSpace(company.ContactSurname);

    private static string EnsureExternalId(Company company)
    {
        if (!string.IsNullOrWhiteSpace(company.SubMerchantExternalId))
            return company.SubMerchantExternalId;

        company.SubMerchantExternalId = $"COMP-{company.Id}";
        return company.SubMerchantExternalId;
    }

    private static string NormalizeIban(string iban)
        => new string(iban.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

    private static object MapSeller(Company company)
        => new
        {
            company.Id,
            company.Name,
            company.Email,
            company.Phone,
            company.Address,
            company.TaxNumber,
            company.MarketplaceEnabled,
            company.SellerLegalTypeId,
            sellerLegalType = SellerLegalTypes.GetById(company.SellerLegalTypeId)?.SystemName,
            company.SellerOnboardingStatusId,
            onboardingStatus = SellerOnboardingStatuses.GetById(company.SellerOnboardingStatusId)?.SystemName,
            company.PlatformCommissionRate,
            company.PayoutDelayDays,
            company.LegalCompanyTitle,
            company.TaxOffice,
            company.Iban,
            company.ContactName,
            company.ContactSurname,
            company.SubMerchantExternalId,
            hasSubMerchantKey = !string.IsNullOrWhiteSpace(company.SubMerchantKey),
            company.OnboardingErrorCode,
            company.OnboardingErrorMessage,
            company.OnboardedAt,
            readyForOnboarding = IsSellerSettingsComplete(company)
        };

    private static object MapTransaction(PaymentTransaction transaction)
        => new
        {
            transaction.Id,
            transaction.ReservationId,
            transaction.CompanyId,
            companyName = transaction.Company.Name,
            visitorName = $"{transaction.Visitor.FirstName} {transaction.Visitor.LastName}",
            tourName = transaction.Reservation.Tour.Name,
            transaction.TypeId,
            type = PaymentTransactionTypes.GetById(transaction.TypeId)?.SystemName,
            transaction.StatusId,
            status = MarketplaceTransactionStatuses.GetById(transaction.StatusId)?.SystemName,
            transaction.Provider,
            transaction.Currency,
            transaction.ConversationId,
            transaction.PaymentId,
            transaction.GrossAmount,
            transaction.PaidAmount,
            transaction.SellerReceivableAmount,
            transaction.PlatformCommissionAmount,
            transaction.PlatformCommissionRate,
            transaction.IyziCommissionRateAmount,
            transaction.IyziCommissionFee,
            transaction.RefundedAmount,
            transaction.PaidAt,
            transaction.CreatedAt,
            lineItems = transaction.LineItems.Select(i => new
            {
                i.Id,
                i.ItemName,
                i.ProviderPaymentTransactionId,
                i.Price,
                i.PaidPrice,
                i.SubMerchantPrice,
                i.SubMerchantPayoutAmount,
                i.MerchantPayoutAmount,
                i.IyziCommissionRateAmount,
                i.IyziCommissionFee
            })
        };

    private static object MapRefund(MarketplaceRefund refund)
        => new
        {
            refund.Id,
            refund.PaymentTransactionId,
            refund.ReservationId,
            refund.CompanyId,
            companyName = refund.Company.Name,
            tourName = refund.Reservation.Tour.Name,
            refund.StatusId,
            status = MarketplaceRefundStatuses.GetById(refund.StatusId)?.SystemName,
            refund.Amount,
            refund.Currency,
            refund.Reason,
            refund.ProviderRefundId,
            refund.ProviderPaymentTransactionId,
            refund.ErrorCode,
            refund.ErrorMessage,
            refund.RequestedAt,
            refund.ProcessedAt
        };

    private static object MapPayout(PayoutBatch payout)
        => new
        {
            payout.Id,
            payout.CompanyId,
            companyName = payout.Company.Name,
            payout.StatusId,
            status = PayoutStatuses.GetById(payout.StatusId)?.SystemName,
            payout.BatchNumber,
            payout.PeriodStart,
            payout.PeriodEnd,
            payout.GrossAmount,
            payout.PlatformCommissionAmount,
            payout.RefundAmount,
            payout.NetAmount,
            payout.BankReference,
            payout.Notes,
            payout.ApprovedAt,
            payout.PaidAt,
            payout.CreatedAt,
            entryCount = payout.LedgerEntries.Count
        };
}
