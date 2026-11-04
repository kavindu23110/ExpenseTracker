using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveItemMeasure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizeDismissed",
                table: "BillItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "BillItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "MarkedPrice",
                table: "BillItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "BillItems",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MarkedPrice",
                table: "BillItems",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SizeDismissed",
                table: "BillItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
