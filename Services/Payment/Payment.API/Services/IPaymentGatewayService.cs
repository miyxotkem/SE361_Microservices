namespace Payment.API.Services
{
    public interface IPaymentGatewayService
    {
        string GeneratePaymentUrl(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null);
        bool ValidateWebhook(object webhookData);
    }
}
