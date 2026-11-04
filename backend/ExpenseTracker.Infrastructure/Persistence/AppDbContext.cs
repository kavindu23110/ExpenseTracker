using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Categorisation;
using ExpenseTracker.Domain.Household;
using ExpenseTracker.Domain.PriceWatch;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

/// <summary>The SQL Server database for bills and price reference data.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillFile> BillFiles => Set<BillFile>();
    public DbSet<LearnedMerchant> LearnedMerchants => Set<LearnedMerchant>();
    public DbSet<ItemMapping> ItemMappings => Set<ItemMapping>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<WhatsAppMessage> WhatsAppMessages => Set<WhatsAppMessage>();
    public DbSet<WhatsAppConfig> WhatsAppConfigs => Set<WhatsAppConfig>();
    public DbSet<ToBuyMonthlyFill> ToBuyMonthlyFills => Set<ToBuyMonthlyFill>();
    public DbSet<CustomCategory> CustomCategories => Set<CustomCategory>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<RecurringBill> RecurringBills => Set<RecurringBill>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<MarketPrice> MarketPrices => Set<MarketPrice>();
    public DbSet<InflationRate> InflationRates => Set<InflationRate>();
    public DbSet<PriceTimeline> PriceTimelines => Set<PriceTimeline>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.UseCollation("Latin1_General_100_CI_AS_SC");

        b.Entity<Bill>(e =>
        {
            e.Property(x => x.Lang).HasMaxLength(2);
            e.Property(x => x.Merchant).HasMaxLength(200);
            e.Property(x => x.MerchantSi).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(400);
            e.Property(x => x.AddressSi).HasMaxLength(400);
            e.Property(x => x.BillNo).HasMaxLength(100);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Payment).HasMaxLength(100);
            e.Property(x => x.PaymentSi).HasMaxLength(100);
            e.Property(x => x.FileName).HasMaxLength(260);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Paid).HasPrecision(18, 2);
            e.HasIndex(x => x.Date);
            e.HasOne(x => x.File).WithOne().HasForeignKey<BillFile>(f => f.BillId).OnDelete(DeleteBehavior.Cascade);
            e.OwnsMany(x => x.Items, i =>
            {
                i.ToTable("BillItems");
                i.WithOwner().HasForeignKey("BillId");
                i.HasKey(x => x.Id);
                i.Property(x => x.Name).HasMaxLength(300);
                i.Property(x => x.NameSi).HasMaxLength(300);
                i.Property(x => x.Unit).HasMaxLength(20);
                i.Property(x => x.MarketKey).HasMaxLength(100);
                i.Property(x => x.Qty).HasPrecision(18, 3);
                i.Property(x => x.Price).HasPrecision(18, 2);
                i.Property(x => x.MarkedPrice).HasPrecision(18, 2);
            });
            e.OwnsMany(x => x.Charges, c =>
            {
                c.ToTable("BillCharges");
                c.WithOwner().HasForeignKey("BillId");
                c.HasKey(x => x.Id);
                c.Property(x => x.Label).HasMaxLength(200);
                c.Property(x => x.LabelSi).HasMaxLength(200);
                c.Property(x => x.Amount).HasPrecision(18, 2);
            });
        });

        b.Entity<BillFile>(e => e.Property(x => x.ContentType).HasMaxLength(50));

        b.Entity<LearnedMerchant>(e =>
        {
            e.HasKey(x => x.Merchant);
            e.Property(x => x.Merchant).HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(50);
        });

        b.Entity<ItemMapping>(e =>
        {
            e.HasKey(x => new { x.KeyA, x.KeyB });
            e.Property(x => x.KeyA).HasMaxLength(200);
            e.Property(x => x.KeyB).HasMaxLength(200);
        });

        b.Entity<CustomCategory>(e =>
        {
            e.HasKey(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(BillCategory.MaxLength);
        });

        b.Entity<ToBuyMonthlyFill>(e =>
        {
            e.HasKey(x => x.Month);
            e.Property(x => x.Month).HasMaxLength(7);
        });

        b.Entity<Budget>(e =>
        {
            e.HasKey(x => x.Category);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.MonthlyLimit).HasPrecision(18, 2);
        });

        b.Entity<SavingsGoal>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Target).HasPrecision(18, 2);
            e.Property(x => x.Saved).HasPrecision(18, 2);
        });

        b.Entity<RecurringBill>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Amount).HasPrecision(18, 2);
        });

        b.Entity<PantryItem>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(200);
            e.Property(x => x.Name).HasMaxLength(300);
            e.Property(x => x.Unit).HasMaxLength(20);
            e.Property(x => x.Qty).HasPrecision(18, 3);
            e.Ignore(x => x.IsOut);
        });

        b.Entity<ShoppingItem>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(200);
            e.Property(x => x.Name).HasMaxLength(300);
            e.Property(x => x.Qty).HasPrecision(18, 3);
            e.Property(x => x.Unit).HasMaxLength(20);
        });

        b.Entity<WhatsAppConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.PhoneNumberId).HasMaxLength(50);
        });

        b.Entity<WhatsAppMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(200);
        });

        b.Entity<AppUser>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50);
            e.Property(x => x.PasswordHash).HasMaxLength(500);
            e.Property(x => x.WhatsAppNumber).HasMaxLength(15);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<MarketPrice>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Unit).HasMaxLength(20);
            e.Property(x => x.Label).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(200);
        });

        b.Entity<InflationRate>(e =>
        {
            e.Property(x => x.Label).HasMaxLength(100);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Note).HasMaxLength(100);
            e.Property(x => x.Rate).HasPrecision(5, 2);
            e.Property(x => x.PreviousRate).HasPrecision(5, 2);
        });

        b.Entity<PriceTimeline>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Summary).HasMaxLength(400);
            e.Property(x => x.Foot).HasMaxLength(400);
            e.Property(x => x.BasePrice).HasPrecision(18, 2);
            e.OwnsMany(x => x.Points, p =>
            {
                p.ToTable("PriceTimelinePoints");
                p.WithOwner().HasForeignKey("PriceTimelineId");
                p.HasKey(x => x.Id);
                p.Property(x => x.Label).HasMaxLength(200);
                p.Property(x => x.Price).HasPrecision(18, 2);
            });
        });
    }
}
