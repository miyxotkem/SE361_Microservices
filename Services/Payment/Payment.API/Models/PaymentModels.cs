namespace Payment.API.Models
{
    public class PaymentRequest
    {
        public string CourseId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "VNPay"; // VNPay, MoMo, PayPal
        public string? VoucherCode { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; }
    }

    public class TransactionRecord
    {
        public string TransactionId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public string? VoucherCode { get; set; }
    }

    public class Voucher
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
