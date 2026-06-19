namespace Payment.API.Services
{
    public class MoMoService : IPaymentGatewayService
    {
        public string GeneratePaymentUrl(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null)
        {
            // Example/Mock URL generation for MoMo
            var baseUrl = "https://test-payment.momo.vn/v2/gateway/api/create";
            var mockUrl = $"{baseUrl}?orderId={transactionId}&amount={amount}&orderInfo=Thanh_toan_khoa_hoc_{courseId}";
            return mockUrl;
        }

        public bool ValidateWebhook(object webhookData)
        {
            // Logic to validate MoMo signature
            return true;
        }
    }
}
