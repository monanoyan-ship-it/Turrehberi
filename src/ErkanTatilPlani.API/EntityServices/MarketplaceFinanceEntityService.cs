using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.EntityServices;
using ErkanTatilPlani.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.API.EntityServices;

public class MarketplaceFinanceEntityService : IMarketplaceFinanceEntityService
{
    private readonly AppDbContext _context;

    public MarketplaceFinanceEntityService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<PaymentTransaction> GetTransactions()
        => _context.PaymentTransactions.Where(t => t.IsActive);

    public IQueryable<PaymentLineItem> GetLineItems()
        => _context.PaymentLineItems.Where(i => i.IsActive);

    public IQueryable<MarketplaceLedgerEntry> GetLedgerEntries()
        => _context.MarketplaceLedgerEntries.Where(e => e.IsActive);

    public IQueryable<MarketplaceRefund> GetRefunds()
        => _context.MarketplaceRefunds.Where(r => r.IsActive);

    public IQueryable<PayoutBatch> GetPayoutBatches()
        => _context.PayoutBatches.Where(p => p.IsActive);

    public async Task<PaymentTransaction?> GetTransactionByIdAsync(int id)
        => await _context.PaymentTransactions
            .Include(t => t.LineItems)
            .Include(t => t.LedgerEntries)
            .Include(t => t.Refunds)
            .Include(t => t.Reservation).ThenInclude(r => r.Tour)
            .Include(t => t.Company)
            .Include(t => t.Visitor)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

    public async Task<PaymentTransaction?> GetTransactionByTokenAsync(string token)
        => await _context.PaymentTransactions
            .Include(t => t.LineItems)
            .Include(t => t.LedgerEntries)
            .Include(t => t.Refunds)
            .Include(t => t.Reservation).ThenInclude(r => r.Tour)
            .Include(t => t.Company)
            .Include(t => t.Visitor)
            .FirstOrDefaultAsync(t => t.PaymentToken == token && t.IsActive);

    public async Task<PaymentTransaction?> GetLatestTransactionForReservationAsync(int reservationId)
        => await _context.PaymentTransactions
            .Include(t => t.LineItems)
            .Include(t => t.LedgerEntries)
            .Include(t => t.Refunds)
            .Where(t => t.ReservationId == reservationId && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<PayoutBatch?> GetPayoutBatchByIdAsync(int id)
        => await _context.PayoutBatches
            .Include(p => p.Company)
            .Include(p => p.LedgerEntries)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public void AddTransaction(PaymentTransaction transaction) => _context.PaymentTransactions.Add(transaction);

    public void AddLineItem(PaymentLineItem lineItem) => _context.PaymentLineItems.Add(lineItem);

    public void AddLedgerEntry(MarketplaceLedgerEntry entry) => _context.MarketplaceLedgerEntries.Add(entry);

    public void AddRefund(MarketplaceRefund refund) => _context.MarketplaceRefunds.Add(refund);

    public void AddPayoutBatch(PayoutBatch payoutBatch) => _context.PayoutBatches.Add(payoutBatch);

    public void UpdateTransaction(PaymentTransaction transaction) => _context.Entry(transaction).State = EntityState.Modified;

    public void UpdateLineItem(PaymentLineItem lineItem) => _context.Entry(lineItem).State = EntityState.Modified;

    public void UpdateLedgerEntry(MarketplaceLedgerEntry entry) => _context.Entry(entry).State = EntityState.Modified;

    public void UpdateRefund(MarketplaceRefund refund) => _context.Entry(refund).State = EntityState.Modified;

    public void UpdatePayoutBatch(PayoutBatch payoutBatch) => _context.Entry(payoutBatch).State = EntityState.Modified;
}
