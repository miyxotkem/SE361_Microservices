using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Payment.API.Models;
using Payment.API.Services;
using BuildingBlocks.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Microsoft.AspNetCore.Authorization;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IEnumerable<IPaymentGatewayService> _paymentServices;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMemoryCache _cache;
        private readonly PaymentDbContext _dbContext;

        public PaymentController(IEnumerable<IPaymentGatewayService> paymentServices, IPublishEndpoint publishEndpoint, IMemoryCache cache, PaymentDbContext dbContext)
        {
            _paymentServices = paymentServices;
            _publishEndpoint = publishEndpoint;
            _cache = cache;
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            // 1. Database Voucher validation
            decimal finalAmount = request.Amount;
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.Code == request.VoucherCode && v.IsActive && v.ExpiryDate > DateTime.UtcNow);
                if (voucher != null)
                {
                    var discount = request.Amount * (voucher.DiscountPercentage / 100m);
                    if (voucher.MaxDiscountAmount > 0 && discount > voucher.MaxDiscountAmount)
                    {
                        discount = voucher.MaxDiscountAmount;
                    }
                    finalAmount = request.Amount - discount;
                }
            }

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

            // Save Pending Transaction Record to Database
            var transaction = new TransactionRecord
            {
                TransactionId = correlationId.ToString(),
                CourseId = request.CourseId,
                UserId = request.UserId,
                Amount = finalAmount,
                PaymentMethod = request.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                VoucherCode = request.VoucherCode
            };
            _dbContext.TransactionRecords.Add(transaction);
            await _dbContext.SaveChangesAsync();

            // Publish PaymentInitiatedEvent
            await _publishEndpoint.Publish(new PaymentInitiatedEvent
            {
                CorrelationId = correlationId,
                UserId = request.UserId,
                CourseId = request.CourseId,
                Amount = finalAmount
            });

            var paymentUrl = await service.GeneratePaymentUrlAsync(correlationId.ToString(), finalAmount, request.CourseId, request.UserId, request.ReturnUrl);

            // 4. Lưu mapping txnRef (yyyyMMddHHmmss) -> correlationId vào cache (hết hạn sau 30 phút)
            //    VNPay trả về vnp_TxnRef trong callback, cần tra cứu để lấy correlationId
            var txnRef = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            if (request.PaymentMethod == "VNPay")
            {
                _cache.Set($"txn:{txnRef}", correlationId, TimeSpan.FromMinutes(30));
            }
            else if (request.PaymentMethod == "PayPal" && !string.IsNullOrEmpty(paymentUrl))
            {
                try
                {
                    var uri = new Uri(paymentUrl);
                    var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var token = queryParams["token"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        _cache.Set($"txn:{token}", correlationId, TimeSpan.FromMinutes(30));
                    }
                }
                catch { }
            }

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
            public string GatewayOrderId { get; set; } // Added for PayPal capture
            public decimal Amount { get; set; }
        }

        [HttpPost("webhook/{method}")]
        [AllowAnonymous]
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
                    // Thử tra cứu bằng GatewayOrderId (dùng cho PayPal)
                    if (string.IsNullOrEmpty(payload.GatewayOrderId) || !_cache.TryGetValue($"txn:{payload.GatewayOrderId}", out correlationId))
                    {
                        // Không tìm được mapping, nhưng vẫn xử lý nếu có đủ UserId/CourseId
                        // Tạo correlationId mới để tiếp tục flow
                        correlationId = Guid.NewGuid();
                    }
                }
            }

            // In reality, verify the payment signature with VNPay before proceeding.
            // Assuming successful payment for demo purposes:
            bool isSuccess = true; 

            if (method.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
            {
                var paypalService = _paymentServices.OfType<PayPalService>().FirstOrDefault();
                if (paypalService != null && !string.IsNullOrEmpty(payload.GatewayOrderId))
                {
                    isSuccess = await paypalService.CaptureOrderAsync(payload.GatewayOrderId);
                }
                else
                {
                    isSuccess = false;
                }
            }

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
                    Reason = "Payment gateway verification or capture failed."
                });
            }

            // Update Transaction Record in Database
            var transaction = await _dbContext.TransactionRecords.FirstOrDefaultAsync(t => t.TransactionId == correlationId.ToString());
            if (transaction != null)
            {
                transaction.Status = isSuccess ? "Success" : "Failed";
                await _dbContext.SaveChangesAsync();
            }

            return Ok(new { Message = "Webhook processed successfully" });
        }

        [HttpGet("paypal/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalReturn([FromQuery] string token, [FromQuery] string PayerID, [FromQuery] string transactionId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(transactionId))
            {
                return BadRequest("Invalid return parameters.");
            }

            var paypalService = _paymentServices.OfType<PayPalService>().FirstOrDefault();
            if (paypalService == null)
            {
                return StatusCode(500, "PayPal service is not configured.");
            }

            // Capture the order using the token (which is the orderId in PayPal v2)
            bool isSuccess = await paypalService.CaptureOrderAsync(token);

            Guid correlationId;
            if (!Guid.TryParse(transactionId, out correlationId))
            {
                correlationId = Guid.NewGuid();
            }

            // Update Transaction Record in Database
            var transaction = await _dbContext.TransactionRecords.FirstOrDefaultAsync(t => t.TransactionId == correlationId.ToString());
            if (transaction != null)
            {
                transaction.Status = isSuccess ? "Success" : "Failed";
                await _dbContext.SaveChangesAsync();
            }

            if (isSuccess)
            {
                await _publishEndpoint.Publish(new PaymentCompletedEvent
                {
                    CorrelationId = correlationId,
                    TransactionId = transactionId
                });
                
                // Trả về HTML đóng trình duyệt hoặc hiển thị thành công
                return Content("<html><body><h2>Thanh toán PayPal thành công!</h2><p>Vui lòng quay lại ứng dụng E-Learning.</p><script>setTimeout(function(){ window.close(); }, 3000);</script></body></html>", "text/html");
            }
            else
            {
                await _publishEndpoint.Publish(new PaymentFailedEvent
                {
                    CorrelationId = correlationId,
                    Reason = "PayPal capture failed."
                });
                
                return Content("<html><body><h2>Thanh toán thất bại!</h2><p>Vui lòng quay lại ứng dụng và thử lại.</p></body></html>", "text/html");
            }
        }
    }
}
