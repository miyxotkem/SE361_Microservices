using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Vouchers",
                keyColumn: "Code",
                keyValue: "DISCOUNT20",
                column: "ExpiryDate",
                value: new DateTime(2036, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Vouchers",
                keyColumn: "Code",
                keyValue: "DISCOUNT20",
                column: "ExpiryDate",
                value: new DateTime(2036, 6, 25, 4, 8, 11, 448, DateTimeKind.Utc).AddTicks(3560));
        }
    }
}
