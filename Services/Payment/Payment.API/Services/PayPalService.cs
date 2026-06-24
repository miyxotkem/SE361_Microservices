using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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
                : $"http://localhost:7000/api/payment/paypal/return?transactionId={transactionId}";
            var cancelUrl = "http://localhost:7000/api/payment/paypal/cancel";

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
        
        public async Task<bool> CaptureOrderAsync(string orderId)
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
                    return false;
                }
                
                var jsonString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonString);
                var status = document.RootElement.GetProperty("status").GetString();
                
                return status == "COMPLETED";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing PayPal order: {ex.Message}");
                return false;
            }
        }

        public bool ValidateWebhook(object webhookData)
        {
            return true;
        }
    }
}
