using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Payment.API.Models;
using Payment.API.Services;
using BuildingBlocks.Messaging.Events;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IEnumerable<IPaymentGatewayService> _paymentServices;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMemoryCache _cache;

        public PaymentController(IEnumerable<IPaymentGatewayService> paymentServices, IPublishEndpoint publishEndpoint, IMemoryCache cache)
        {
            _paymentServices = paymentServices;
            _publishEndpoint = publishEndpoint;
            _cache = cache;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            // 1. Mock Voucher logic
            decimal finalAmount = request.Amount;
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                // In reality, validate voucher from DB
                if (request.VoucherCode == "DISCOUNT20")
                {
                    finalAmount = request.Amount * 0.8m; // 20% discount
                }
            }

            var transactionId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid();

            // 2. Select Payment Service
            IPaymentGatewayService? service = request.PaymentMethod switch
            {
                "VNPay" => _paymentServices.OfType<VnPayService>().FirstOrDefault(),
                "MoMo" => _paymentServices.OfType<MoMoService>().FirstOrDefault(),
                "PayPal" => _paymentServices.OfType<PayPalService>().FirstOrDefault(),
                _ => null
            };

            if (service == null)
            {
                return BadRequest(new { Message = "Unsupported payment method" });
            }

            // Publish PaymentInitiatedEvent
            await _publishEndpoint.Publish(new PaymentInitiatedEvent
            {
                CorrelationId = correlationId,
                UserId = request.UserId,
                CourseId = request.CourseId,
                Amount = finalAmount
            });

            // 3. Generate URL
            var paymentUrl = service.GeneratePaymentUrl(correlationId.ToString(), finalAmount, request.CourseId, request.UserId, request.ReturnUrl);

            // 4. Lưu mapping txnRef (yyyyMMddHHmmss) -> correlationId vào cache (hết hạn sau 30 phút)
            //    VNPay trả về vnp_TxnRef trong callback, cần tra cứu để lấy correlationId
            var txnRef = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            _cache.Set($"txn:{txnRef}", correlationId, TimeSpan.FromMinutes(30));
            // Cũng lưu theo correlationId để fallback
            _cache.Set($"corr:{correlationId}", txnRef, TimeSpan.FromMinutes(30));

            return Ok(new PaymentResponse
            {
                Success = true,
                PaymentUrl = paymentUrl,
                Message = "Payment URL generated successfully.",
                CorrelationId = correlationId
            });
        }

        public class WebhookPayload
        {
            public string UserId { get; set; }
            public string CourseId { get; set; }
            public string TransactionId { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost("webhook/{method}")]
        public async Task<IActionResult> Webhook(string method, [FromBody] WebhookPayload payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.UserId) || string.IsNullOrEmpty(payload.CourseId))
            {
                return BadRequest(new { Message = "Invalid payload" });
            }

            // TransactionId có thể là:
            // - GUID (correlationId trực tiếp)
            // - chuỗi yyyyMMddHHmmss (vnp_TxnRef từ VNPay callback)
            Guid correlationId;
            if (!Guid.TryParse(payload.TransactionId, out correlationId))
            {
                // Thử tra cứu mapping txnRef -> correlationId từ cache
                if (!_cache.TryGetValue($"txn:{payload.TransactionId}", out correlationId))
                {
                    // Không tìm được mapping, nhưng vẫn xử lý nếu có đủ UserId/CourseId
                    // Tạo correlationId mới để tiếp tục flow
                    correlationId = Guid.NewGuid();
                }
            }

            // In reality, verify the payment signature with VNPay before proceeding.
            // Assuming successful payment for demo purposes:
            bool isSuccess = true; // In real code: bool isSuccess = ValidateVnPaySignature(Request.Query);

            if (isSuccess)
            {
                await _publishEndpoint.Publish(new PaymentCompletedEvent
                {
                    CorrelationId = correlationId,
                    TransactionId = payload.TransactionId
                });
            }
            else
            {
                await _publishEndpoint.Publish(new PaymentFailedEvent
                {
                    CorrelationId = correlationId,
                    Reason = "Payment gateway verification failed."
                });
            }

            return Ok(new { Message = "Webhook processed successfully" });
        }
    }
}
