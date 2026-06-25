using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Payment.API.Models;

namespace Payment.API.Services
{
    public class MoMoService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MoMoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<string> GeneratePaymentUrlAsync(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null)
        {
            var momoConfig = _configuration.GetSection("MoMo");
            var partnerCode = momoConfig["PartnerCode"] ?? "";
            var accessKey = momoConfig["AccessKey"] ?? "";
            var secretKey = momoConfig["SecretKey"] ?? "";
            var endpoint = momoConfig["Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
            var notifyUrl = momoConfig["NotifyUrl"] ?? "http://localhost:5000/api/payment/webhook/MoMo";

            string orderId = transactionId;
            string requestId = Guid.NewGuid().ToString();
            string orderInfo = $"Thanh toan khoa hoc {courseId}";
            string returnUrlFinal = returnUrl ?? momoConfig["ReturnUrl"] ?? notifyUrl;
            long amountLong = (long)amount;
            string extraData = "";

            string requestType = "payWithMethod";
            string rawHash = $"accessKey={accessKey}&amount={amountLong}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrlFinal}&requestId={requestId}&requestType={requestType}";

            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash));
                signature = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }

            var requestData = new
            {
                partnerCode,
                partnerName = "MoMo Payment",
                storeId = "Test Store",
                requestId,
                amount = amountLong,
                orderId,
                orderInfo,
                redirectUrl = returnUrlFinal,
                ipnUrl = notifyUrl,
                lang = "vi",
                extraData,
                requestType = requestType,
                orderGroupId = "",
                autoCapture = true,
                signature
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseString);
            if (document.RootElement.TryGetProperty("payUrl", out var payUrlElement))
            {
                return payUrlElement.GetString() ?? "";
            }

            throw new Exception($"MoMo URL generation failed. Response: {responseString}");
        }

        public bool ValidateWebhook(object webhookData)
        {
            return true;
        }

        public async Task<RefundResult> RefundAsync(
            string transactionId, 
            decimal amount, 
            string reason, 
            string? gatewayTransactionId, 
            string? gatewayOrderId, 
            DateTime originalCreatedAt)
        {
            try
            {
                if (string.IsNullOrEmpty(gatewayTransactionId))
                {
                    return new RefundResult { Success = false, Message = "Missing MoMo Transaction ID (GatewayTransactionId)" };
                }

                var momoConfig = _configuration.GetSection("MoMo");
                var partnerCode = momoConfig["PartnerCode"] ?? "";
                var accessKey = momoConfig["AccessKey"] ?? "";
                var secretKey = momoConfig["SecretKey"] ?? "";
                var endpoint = momoConfig["Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
                var refundEndpoint = endpoint.Replace("/create", "/refund");

                string orderId = $"refund-{transactionId}-{DateTime.UtcNow.Ticks}";
                string requestId = Guid.NewGuid().ToString();
                long amountLong = (long)amount;
                
                if (!long.TryParse(gatewayTransactionId, out long transIdLong))
                {
                    return new RefundResult { Success = false, Message = $"Invalid MoMo Transaction ID format: {gatewayTransactionId}" };
                }

                string rawHash = $"accessKey={accessKey}&amount={amountLong}&description={reason}&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}&transId={transIdLong}";

                string signature;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                {
                    byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash));
                    signature = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                }

                var requestData = new
                {
                    partnerCode,
                    requestId,
                    amount = amountLong,
                    orderId,
                    transId = transIdLong,
                    description = reason,
                    signature
                };

                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(refundEndpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(responseString);
                if (document.RootElement.TryGetProperty("resultCode", out var resultCodeElement))
                {
                    int resultCode = resultCodeElement.GetInt32();
                    if (resultCode == 0)
                    {
                        string refundTransId = gatewayTransactionId;
                        if (document.RootElement.TryGetProperty("transId", out var tId))
                        {
                            if (tId.ValueKind == JsonValueKind.Number)
                                refundTransId = tId.GetInt64().ToString();
                            else if (tId.ValueKind == JsonValueKind.String)
                                refundTransId = tId.GetString() ?? gatewayTransactionId;
                        }

                        return new RefundResult
                        {
                            Success = true,
                            Message = "Refund processed successfully via MoMo",
                            GatewayRefundId = refundTransId
                        };
                    }

                    string message = document.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                    return new RefundResult { Success = false, Message = $"MoMo refund failed (Code: {resultCode}): {message}" };
                }

                return new RefundResult { Success = false, Message = $"MoMo refund failed, response: {responseString}" };
            }
            catch (Exception ex)
            {
                return new RefundResult { Success = false, Message = $"MoMo refund exception: {ex.Message}" };
            }
        }
    }
}
