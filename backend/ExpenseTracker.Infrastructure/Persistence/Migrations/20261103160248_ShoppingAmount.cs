using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Qty",
                table: "ShoppingItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ShoppingItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Qty",
                table: "ShoppingItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ShoppingItems");
        }
    }
}
