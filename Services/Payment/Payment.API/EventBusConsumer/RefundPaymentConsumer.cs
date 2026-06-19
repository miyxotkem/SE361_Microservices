using BuildingBlocks.Messaging.Commands;
using MassTransit;

namespace Payment.API.EventBusConsumer;

public class RefundPaymentConsumer : IConsumer<RefundPaymentCommand>
{
    private readonly ILogger<RefundPaymentConsumer> _logger;

    public RefundPaymentConsumer(ILogger<RefundPaymentConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RefundPaymentCommand> context)
    {
        _logger.LogInformation("Processing refund for TransactionId: {TransactionId}, Reason: {Reason}", context.Message.TransactionId, context.Message.Reason);

        // Call VNPay Refund API here. For now we just mock it.
        await Task.Delay(100);

        _logger.LogInformation("Refund processed successfully for TransactionId: {TransactionId}", context.Message.TransactionId);
    }
}
