using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <summary>Read-only views for Superset charts, with each item already under its own category.</summary>
    public partial class ReportingViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE VIEW dbo.ReportBillLines AS
                SELECT b.Id AS BillId, b.[Date] AS BillDate, b.Merchant AS Shop, b.Category AS BillCategory,
                       i.Name AS Item, i.NameSi AS ItemSinhala, COALESCE(i.Category, b.Category) AS Category,
                       i.Qty, i.Unit, i.Price, ROUND(i.Qty * i.Price, 2) AS Amount, b.PaidBy, b.Scanned, b.Lang AS BillLanguage
                FROM dbo.BillItems i JOIN dbo.Bills b ON b.Id = i.BillId
                UNION ALL
                SELECT b.Id, b.[Date], b.Merchant, b.Category, c.Label, c.LabelSi, b.Category,
                       NULL, NULL, NULL, c.Amount, b.PaidBy, b.Scanned, b.Lang
                FROM dbo.BillCharges c JOIN dbo.Bills b ON b.Id = c.BillId
                """);
            migrationBuilder.Sql("""
                CREATE VIEW dbo.ReportBills AS
                SELECT Id AS BillId, [Date] AS BillDate, Merchant AS Shop, Category, Amount, PaidBy, Scanned, Lang AS BillLanguage, Payment
                FROM dbo.Bills
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.ReportBills");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.ReportBillLines");
        }
    }
}
