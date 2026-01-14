using ErkanTatilPlani.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TaxNumber).IsUnique();
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Tours)
                  .HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.Reservations)
                  .HasForeignKey(e => e.TourId);
            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.Reservations)
                  .HasForeignKey(e => e.VisitorId);
        });
    }
}
