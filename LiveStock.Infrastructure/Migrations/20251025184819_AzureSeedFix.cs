using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveStock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AzureSeedFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PurchaseDate" },
                values: new object[] { new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "PurchaseDate" },
                values: new object[] { new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "FinancialRecords",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "TransactionDate" },
                values: new object[] { new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "FinancialRecords",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "TransactionDate" },
                values: new object[] { new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "FinancialRecords",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "TransactionDate" },
                values: new object[] { new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PurchaseDate" },
                values: new object[] { new DateTime(2025, 8, 26, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(3010), new DateTime(2025, 8, 26, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(1230) });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "PurchaseDate" },
                values: new object[] { new DateTime(2025, 9, 25, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(4240), new DateTime(2026, 4, 25, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(3570), new DateTime(2025, 9, 25, 18, 45, 57, 756, DateTimeKind.Utc).AddTicks(3560) });

            migrationBuilder.UpdateData(
                table: "FinancialRecords",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "TransactionDate" },
                values: new object[] { new DateTime(2025, 10, 15, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4200), new DateTime(2025, 10, 15, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(2410) });

            migrationBuilder.UpdateData(
                table: "FinancialRecords",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "TransactionDate" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770), new DateTime(2025, 10, 10, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770) });

            migrationBuilder.UpdateData(
                table: "FinancialRecords",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "TransactionDate" },
                values: new object[] { new DateTime(2025, 10, 5, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770), new DateTime(2025, 10, 5, 18, 45, 57, 755, DateTimeKind.Utc).AddTicks(4770) });

            migrationBuilder.UpdateData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 25, 18, 45, 57, 754, DateTimeKind.Utc).AddTicks(8340));
        }
    }
}
