using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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