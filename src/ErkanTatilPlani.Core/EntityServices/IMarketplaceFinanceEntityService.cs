using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.EntityServices;

public interface IMarketplaceFinanceEntityService
{
    IQueryable<PaymentTransaction> GetTransactions();
    IQueryable<PaymentLineItem> GetLineItems();
    IQueryable<MarketplaceLedgerEntry> GetLedgerEntries();
    IQueryable<MarketplaceRefund> GetRefunds();
    IQueryable<PayoutBatch> GetPayoutBatches();
    Task<PaymentTransaction?> GetTransactionByIdAsync(int id);
    Task<PaymentTransaction?> GetTransactionByTokenAsync(string token);
    Task<PaymentTransaction?> GetLatestTransactionForReservationAsync(int reservationId);
    Task<PayoutBatch?> GetPayoutBatchByIdAsync(int id);
    void AddTransaction(PaymentTransaction transaction);
    void AddLineItem(PaymentLineItem lineItem);
    void AddLedgerEntry(MarketplaceLedgerEntry entry);
    void AddRefund(MarketplaceRefund refund);
    void AddPayoutBatch(PayoutBatch payoutBatch);
    void UpdateTransaction(PaymentTransaction transaction);
    void UpdateLineItem(PaymentLineItem lineItem);
    void UpdateLedgerEntry(MarketplaceLedgerEntry entry);
    void UpdateRefund(MarketplaceRefund refund);
    void UpdatePayoutBatch(PayoutBatch payoutBatch);
}
