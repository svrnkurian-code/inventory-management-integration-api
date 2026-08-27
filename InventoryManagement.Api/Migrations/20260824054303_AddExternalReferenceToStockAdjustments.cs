using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalReferenceToStockAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "StockAdjustments",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_ExternalReference",
                table: "StockAdjustments",
                column: "ExternalReference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockAdjustments_ExternalReference",
                table: "StockAdjustments");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "StockAdjustments");
        }
    }
}
