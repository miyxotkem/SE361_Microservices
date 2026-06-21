namespace Payment.API.Services
{
    public interface IPaymentGatewayService
    {
        Task<string> GeneratePaymentUrlAsync(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null);
        bool ValidateWebhook(object webhookData);
    }
}
