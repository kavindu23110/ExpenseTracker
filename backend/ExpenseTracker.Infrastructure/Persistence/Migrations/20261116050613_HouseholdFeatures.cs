using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HouseholdFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Expiry",
                table: "PantryItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "PantryItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LowAt",
                table: "PantryItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaidBy",
                table: "Bills",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlertDigests",
                columns: table => new
                {
                    Day = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertDigests", x => x.Day);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategoryRules",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategoryRules", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    To = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertDigests");

            migrationBuilder.DropTable(
                name: "ItemCategoryRules");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropColumn(
                name: "Expiry",
                table: "PantryItems");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PantryItems");

            migrationBuilder.DropColumn(
                name: "LowAt",
                table: "PantryItems");

            migrationBuilder.DropColumn(
                name: "PaidBy",
                table: "Bills");
        }
    }
}
