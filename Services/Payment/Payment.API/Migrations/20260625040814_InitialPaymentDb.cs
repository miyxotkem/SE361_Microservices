using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPaymentDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionRecords",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    VoucherCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecords", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "Vouchers",
                columns: new[] { "Code", "DiscountPercentage", "ExpiryDate", "IsActive", "MaxDiscountAmount" },
                values: new object[] { "DISCOUNT20", 20m, new DateTime(2036, 6, 25, 4, 8, 11, 448, DateTimeKind.Utc).AddTicks(3560), true, 100000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionRecords");

            migrationBuilder.DropTable(
                name: "Vouchers");
        }
    }
}
