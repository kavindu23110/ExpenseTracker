using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ToBuyMonthlyFill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToBuyMonthlyFills",
                columns: table => new
                {
                    Month = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToBuyMonthlyFills", x => x.Month);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToBuyMonthlyFills");
        }
    }
}
