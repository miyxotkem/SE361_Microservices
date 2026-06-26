using BuildingBlocks.Messaging.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Services;

namespace Payment.API.EventBusConsumer;

public class RefundPaymentConsumer : IConsumer<RefundPaymentCommand>
{
    private readonly ILogger<RefundPaymentConsumer> _logger;
    private readonly PaymentDbContext _dbContext;
    private readonly IEnumerable<IPaymentGatewayService> _paymentServices;

    public RefundPaymentConsumer(
        ILogger<RefundPaymentConsumer> logger, 
        PaymentDbContext dbContext,
        IEnumerable<IPaymentGatewayService> paymentServices)
    {
        _logger = logger;
        _dbContext = dbContext;
        _paymentServices = paymentServices;
    }

    public async Task Consume(ConsumeContext<RefundPaymentCommand> context)
    {
        _logger.LogInformation("Processing refund for TransactionId: {TransactionId}, CorrelationId: {CorrelationId}, Reason: {Reason}", 
            context.Message.TransactionId, context.Message.CorrelationId, context.Message.Reason);

        var transaction = await _dbContext.TransactionRecords.FirstOrDefaultAsync(t => 
            t.TransactionId == context.Message.TransactionId || 
            t.TransactionId == context.Message.CorrelationId.ToString());

        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for refund. TransactionId: {TransactionId}, CorrelationId: {CorrelationId}", 
                context.Message.TransactionId, context.Message.CorrelationId);
            return;
        }

        if (transaction.Status == "Refunded")
        {
            _logger.LogInformation("Transaction {TransactionId} has already been refunded.", transaction.TransactionId);
            return;
        }

        IPaymentGatewayService? service = transaction.PaymentMethod switch
        {
            "VNPay" => _paymentServices.OfType<VnPayService>().FirstOrDefault(),
            "MoMo" => _paymentServices.OfType<MoMoService>().FirstOrDefault(),
            "PayPal" => _paymentServices.OfType<PayPalService>().FirstOrDefault(),
            _ => null
        };

        if (service == null)
        {
            _logger.LogError("Unsupported payment method for refund: {PaymentMethod}", transaction.PaymentMethod);
            return;
        }

        var result = await service.RefundAsync(
            transaction.TransactionId,
            transaction.Amount,
            context.Message.Reason,
            transaction.GatewayTransactionId,
            transaction.GatewayOrderId,
            transaction.CreatedAt
        );

        if (result.Success)
        {
            transaction.Status = "Refunded";
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Refund processed successfully for TransactionId: {TransactionId}. GatewayRefundId: {GatewayRefundId}", 
                transaction.TransactionId, result.GatewayRefundId);
        }
        else
        {
            _logger.LogError("Refund failed for TransactionId: {TransactionId}. Message: {Message}", 
                transaction.TransactionId, result.Message);
        }
    }
}
