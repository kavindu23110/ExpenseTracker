using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ItemMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemMappings",
                columns: table => new
                {
                    KeyA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KeyB = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Same = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMappings", x => new { x.KeyA, x.KeyB });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemMappings");
        }
    }
}
