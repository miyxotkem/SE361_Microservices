using System;
using System.Threading.Tasks;
using Payment.API.Models;

namespace Payment.API.Services
{
    public interface IPaymentGatewayService
    {
        Task<string> GeneratePaymentUrlAsync(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null);
        bool ValidateWebhook(object webhookData);
        Task<RefundResult> RefundAsync(
            string transactionId, 
            decimal amount, 
            string reason, 
            string? gatewayTransactionId, 
            string? gatewayOrderId, 
            DateTime originalCreatedAt);
    }
}
