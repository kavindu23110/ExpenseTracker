using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Lang = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Merchant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MerchantSi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AddressSi = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    BillNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: true),
                    Payment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentSi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Paid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Scanned = table.Column<bool>(type: "bit", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    HasFile = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InflationRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PreviousRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InflationRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearnedMerchants",
                columns: table => new
                {
                    Merchant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnedMerchants", x => x.Merchant);
                });

            migrationBuilder.CreateTable(
                name: "MarketPrices",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPrices", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "PriceTimelines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Foot = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceTimelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LabelSi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillCharges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillCharges_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillFiles_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NameSi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MarkedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MarketKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillItems_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceTimelinePoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsUser = table.Column<bool>(type: "bit", nullable: false),
                    PriceTimelineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceTimelinePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceTimelinePoints_PriceTimelines_PriceTimelineId",
                        column: x => x.PriceTimelineId,
                        principalTable: "PriceTimelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillCharges_BillId",
                table: "BillCharges",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillFiles_BillId",
                table: "BillFiles",
                column: "BillId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillItems_BillId",
                table: "BillItems",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_Date",
                table: "Bills",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTimelinePoints_PriceTimelineId",
                table: "PriceTimelinePoints",
                column: "PriceTimelineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillCharges");

            migrationBuilder.DropTable(
                name: "BillFiles");

            migrationBuilder.DropTable(
                name: "BillItems");

            migrationBuilder.DropTable(
                name: "InflationRates");

            migrationBuilder.DropTable(
                name: "LearnedMerchants");

            migrationBuilder.DropTable(
                name: "MarketPrices");

            migrationBuilder.DropTable(
                name: "PriceTimelinePoints");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "PriceTimelines");
        }
    }
}
