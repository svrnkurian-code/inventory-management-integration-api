using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInboundReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InboundReceiptId",
                table: "StockAdjustments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InboundReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartnerCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ReceiptReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundReceipts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_InboundReceiptId",
                table: "StockAdjustments",
                column: "InboundReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceipts_PartnerCode_ReceiptReference",
                table: "InboundReceipts",
                columns: new[] { "PartnerCode", "ReceiptReference" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustments_InboundReceipts_InboundReceiptId",
                table: "StockAdjustments",
                column: "InboundReceiptId",
                principalTable: "InboundReceipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustments_InboundReceipts_InboundReceiptId",
                table: "StockAdjustments");

            migrationBuilder.DropTable(
                name: "InboundReceipts");

            migrationBuilder.DropIndex(
                name: "IX_StockAdjustments_InboundReceiptId",
                table: "StockAdjustments");

            migrationBuilder.DropColumn(
                name: "InboundReceiptId",
                table: "StockAdjustments");
        }
    }
}
