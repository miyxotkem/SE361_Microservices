using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Payment.API.Models;

namespace Payment.API.Services
{
    public class PayPalService : IPaymentGatewayService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public PayPalService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _config["PayPal:ClientId"];
            var secretKey = _config["PayPal:SecretKey"];
            var apiUrl = _config["PayPal:ApiUrl"];

            var authBytes = Encoding.UTF8.GetBytes($"{clientId}:{secretKey}");
            var authBase64 = Convert.ToBase64String(authBytes);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authBase64);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);
            return document.RootElement.GetProperty("access_token").GetString() ?? "";
        }

        public async Task<string> GeneratePaymentUrlAsync(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null)
        {
            // Convert VND to USD. Fixed rate: 25,000 VND = 1 USD
            decimal usdAmount = Math.Round(amount / 25000m, 2);
            if (usdAmount <= 0) usdAmount = 0.01m; // Minimum 0.01 USD for testing

            var token = await GetAccessTokenAsync();
            var apiUrl = _config["PayPal:ApiUrl"];

            var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // We construct a return URL that our API will handle to capture the payment
            // PayPal appends token (orderId) and PayerID to this URL
            var successUrl = returnUrl != null 
                ? $"{returnUrl}?transactionId={transactionId}" 
                : $"http://20.247.226.105:7000/api/payment/paypal/return?transactionId={transactionId}";
            var cancelUrl = "http://20.247.226.105:7000/api/payment/paypal/cancel";

            var orderPayload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = transactionId, 
                        custom_id = transactionId, // Store transaction id
                        amount = new
                        {
                            currency_code = "USD",
                            value = usdAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        },
                        description = $"Payment for Course {courseId}"
                    }
                },
                application_context = new
                {
                    return_url = successUrl,
                    cancel_url = cancelUrl,
                    user_action = "PAY_NOW" 
                }
            };

            request.Content = new StringContent(JsonSerializer.Serialize(orderPayload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);

            var links = document.RootElement.GetProperty("links").EnumerateArray();
            foreach (var link in links)
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    return link.GetProperty("href").GetString() ?? "";
                }
            }

            throw new Exception("Failed to get approve URL from PayPal");
        }
        
        public async Task<(bool Success, string? CaptureId)> CaptureOrderAsync(string orderId)
        {
            try 
            {
                var token = await GetAccessTokenAsync();
                var apiUrl = _config["PayPal:ApiUrl"];

                var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/v2/checkout/orders/{orderId}/capture");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Content-Type: application/json must be sent even if empty body for some APIs, using an empty object
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json"); 

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"PayPal Capture failed: {error}");
                    return (false, null);
                }
                
                var jsonString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonString);
                var status = document.RootElement.GetProperty("status").GetString();
                
                string? captureId = null;
                if (status == "COMPLETED" && document.RootElement.TryGetProperty("purchase_units", out var purchaseUnits))
                {
                    foreach (var unit in purchaseUnits.EnumerateArray())
                    {
                        if (unit.TryGetProperty("payments", out var payments) && payments.TryGetProperty("captures", out var captures))
                        {
                            foreach (var capture in captures.EnumerateArray())
                            {
                                if (capture.TryGetProperty("id", out var idElement))
                                {
                                    captureId = idElement.GetString();
                                    break;
                                }
                            }
                        }
                        if (captureId != null) break;
                    }
                }

                return (status == "COMPLETED", captureId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing PayPal order: {ex.Message}");
                return (false, null);
            }
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
                    return new RefundResult { Success = false, Message = "Missing PayPal Capture ID (GatewayTransactionId)" };
                }

                decimal usdAmount = Math.Round(amount / 25000m, 2);
                if (usdAmount <= 0) usdAmount = 0.01m;

                var token = await GetAccessTokenAsync();
                var apiUrl = _config["PayPal:ApiUrl"];

                var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/v2/payments/captures/{gatewayTransactionId}/refund");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("PayPal-Request-Id", transactionId); // Idempotency

                var refundPayload = new
                {
                    amount = new
                    {
                        value = usdAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                        currency_code = "USD"
                    },
                    note_to_payer = reason
                };

                request.Content = new StringContent(JsonSerializer.Serialize(refundPayload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new RefundResult { Success = false, Message = $"PayPal Refund API failed: {responseString}" };
                }

                using var document = JsonDocument.Parse(responseString);
                var status = document.RootElement.GetProperty("status").GetString();
                var refundId = document.RootElement.GetProperty("id").GetString();

                if (status == "COMPLETED" || status == "PENDING")
                {
                    return new RefundResult { Success = true, Message = "Refund processed successfully via PayPal", GatewayRefundId = refundId };
                }

                return new RefundResult { Success = false, Message = $"PayPal refund status: {status}" };
            }
            catch (Exception ex)
            {
                return new RefundResult { Success = false, Message = $"PayPal refund exception: {ex.Message}" };
            }
        }

        public bool ValidateWebhook(object webhookData)
        {
            return true;
        }
    }
}
