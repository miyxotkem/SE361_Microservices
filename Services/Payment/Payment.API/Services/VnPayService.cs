using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Payment.API.Models;

namespace Payment.API.Services
{
    public class VnPayService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> GeneratePaymentUrlAsync(string transactionId, decimal amount, string courseId, string userId, string? returnUrl = null)
        {
            var vnpayConfig    = _configuration.GetSection("VnPay");
            var vnp_TmnCode    = vnpayConfig["TmnCode"]    ?? "";
            var vnp_HashSecret = vnpayConfig["HashSecret"] ?? "";
            var vnp_BaseUrl    = vnpayConfig["BaseUrl"]    ?? "";
            var vnp_ReturnUrl  = returnUrl ?? vnpayConfig["ReturnUrl"]  ?? "";

            // Múi giờ Việt Nam UTC+7
            var vnNow = DateTime.UtcNow.AddHours(7);

            // vnp_TxnRef: VNPay chỉ chấp nhận [a-zA-Z0-9], bỏ dấu gạch ngang nếu là GUID

            var txnRef = vnNow.ToString("yyyyMMddHHmmss"); // VD: "20260617004059"

            // vnp_OrderInfo: chỉ dùng chữ/số/khoảng trắng, không ký tự đặc biệt
            var orderInfo = $"Thanh toan don hang {txnRef}";

            var vnp_Params = new SortedList<string, string>(new VnPayCompare())
            {
                { "vnp_Amount",     ((long)(amount * 100)).ToString() },
                { "vnp_Command",    "pay" },
                { "vnp_CreateDate", vnNow.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode",   "VND" },
                { "vnp_ExpireDate", vnNow.AddMinutes(15).ToString("yyyyMMddHHmmss") },
                { "vnp_IpAddr",     "127.0.0.1" },
                { "vnp_Locale",     "vn" },
                { "vnp_OrderInfo",  orderInfo },
                { "vnp_OrderType",  "other" },
                { "vnp_ReturnUrl",  vnp_ReturnUrl },
                { "vnp_TmnCode",    vnp_TmnCode },
                { "vnp_TxnRef",     txnRef },
                { "vnp_Version",    "2.1.0" },
            };

            // ================================================================
            // CHUẨN ĐÚNG (giống hệt thư viện vnpay Node.js):
            //   signData   → key=value KHÔNG encode, nối &, bỏ & cuối
            //   queryString → key=UrlEncode(value), dùng để ghép URL
            // ================================================================
            var signBuilder  = new StringBuilder();
            var queryBuilder = new StringBuilder();

            foreach (var kv in vnp_Params)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    signBuilder.Append($"{kv.Key}={WebUtility.UrlEncode(kv.Value)}&");
                    queryBuilder.Append($"{kv.Key}={WebUtility.UrlEncode(kv.Value)}&");
                }
            }

            string signData = signBuilder.ToString().TrimEnd('&');

            // Bỏ comment để debug nếu vẫn lỗi:
            // Console.WriteLine("=== SIGN DATA ===\n" + signData);

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            string paymentUrl = vnp_BaseUrl + "?" + queryBuilder + "vnp_SecureHash=" + vnp_SecureHash;

            return Task.FromResult(paymentUrl);
        }

        /// <summary>
        /// Xác minh chữ ký VNPay gửi về ReturnUrl / IPN.
        /// </summary>
        public bool ValidateSignature(IDictionary<string, string> vnpayParams)
        {
            var vnpayConfig    = _configuration.GetSection("VnPay");
            var vnp_HashSecret = vnpayConfig["HashSecret"] ?? "";

            if (!vnpayParams.TryGetValue("vnp_SecureHash", out var receivedHash))
                return false;

            var sortedParams = new SortedList<string, string>(new VnPayCompare());
            foreach (var kv in vnpayParams)
            {
                if (kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                    sortedParams[kv.Key] = kv.Value;
            }

            var signBuilder = new StringBuilder();
            foreach (var kv in sortedParams)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                    signBuilder.Append($"{kv.Key}={WebUtility.UrlEncode(kv.Value)}&");
            }

            string signData     = signBuilder.ToString().TrimEnd('&');
            string expectedHash = HmacSHA512(vnp_HashSecret, signData);

            return string.Equals(receivedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
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
                if (string.IsNullOrEmpty(gatewayOrderId))
                {
                    return new RefundResult { Success = false, Message = "Missing VNPay TxnRef (GatewayOrderId)" };
                }

                var vnpayConfig = _configuration.GetSection("VnPay");
                var vnp_TmnCode = vnpayConfig["TmnCode"] ?? "";
                var vnp_HashSecret = vnpayConfig["HashSecret"] ?? "";
                
                var refundUrl = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";

                var vnNow = DateTime.UtcNow.AddHours(7);
                string vnp_RequestId = Guid.NewGuid().ToString();
                string vnp_Version = "2.1.0";
                string vnp_Command = "refund";
                string vnp_TransactionType = "02"; // 02: Full Refund, 03: Partial Refund
                string vnp_TxnRef = gatewayOrderId;
                long vnp_Amount = (long)(amount * 100);
                string vnp_TransactionNo = gatewayTransactionId ?? "0";
                string vnp_TransactionDate = originalCreatedAt.AddHours(7).ToString("yyyyMMddHHmmss");
                string vnp_CreateBy = "System";
                string vnp_CreateDate = vnNow.ToString("yyyyMMddHHmmss");
                string vnp_IpAddr = "127.0.0.1";
                string vnp_OrderInfo = reason;

                // Format: vnp_RequestId|vnp_Version|vnp_Command|vnp_TmnCode|vnp_TransactionType|vnp_TxnRef|vnp_Amount|vnp_TransactionNo|vnp_TransactionDate|vnp_CreateBy|vnp_CreateDate|vnp_IpAddr|vnp_OrderInfo
                string signData = $"{vnp_RequestId}|{vnp_Version}|{vnp_Command}|{vnp_TmnCode}|{vnp_TransactionType}|{vnp_TxnRef}|{vnp_Amount}|{vnp_TransactionNo}|{vnp_TransactionDate}|{vnp_CreateBy}|{vnp_CreateDate}|{vnp_IpAddr}|{vnp_OrderInfo}";

                string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

                var requestData = new
                {
                    vnp_RequestId,
                    vnp_Version,
                    vnp_Command,
                    vnp_TmnCode,
                    vnp_TransactionType,
                    vnp_TxnRef,
                    vnp_Amount,
                    vnp_TransactionNo,
                    vnp_TransactionDate,
                    vnp_CreateBy,
                    vnp_CreateDate,
                    vnp_IpAddr,
                    vnp_OrderInfo,
                    vnp_SecureHash
                };

                using var httpClient = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(refundUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(responseString);
                if (document.RootElement.TryGetProperty("vnp_ResponseCode", out var responseCodeElement))
                {
                    string responseCode = responseCodeElement.GetString() ?? "";
                    if (responseCode == "00")
                    {
                        return new RefundResult
                        {
                            Success = true,
                            Message = "Refund processed successfully via VNPay",
                            GatewayRefundId = document.RootElement.TryGetProperty("vnp_ResponseId", out var respId) ? respId.GetString() : null
                        };
                    }
                    string message = document.RootElement.TryGetProperty("vnp_Message", out var msg) ? msg.GetString() ?? "" : "";
                    return new RefundResult { Success = false, Message = $"VNPay refund failed (Code: {responseCode}): {message}" };
                }

                return new RefundResult { Success = false, Message = $"VNPay refund failed, response: {responseString}" };
            }
            catch (Exception ex)
            {
                return new RefundResult { Success = false, Message = $"VNPay refund exception: {ex.Message}" };
            }
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash      = new StringBuilder();
            byte[] keyBytes   = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                    hash.Append(b.ToString("x2"));
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}