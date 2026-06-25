using BuildingBlocks.Messaging.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;

namespace Payment.API.EventBusConsumer;

public class RefundPaymentConsumer : IConsumer<RefundPaymentCommand>
{
    private readonly ILogger<RefundPaymentConsumer> _logger;
    private readonly PaymentDbContext _dbContext;

    public RefundPaymentConsumer(ILogger<RefundPaymentConsumer> logger, PaymentDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<RefundPaymentCommand> context)
    {
        _logger.LogInformation("Processing refund for TransactionId: {TransactionId}, Reason: {Reason}", context.Message.TransactionId, context.Message.Reason);

        // Call VNPay Refund API here. For now we just mock it.
        await Task.Delay(100);

        // Update Transaction status to Refunded in Database
        var transaction = await _dbContext.TransactionRecords.FirstOrDefaultAsync(t => t.TransactionId == context.Message.TransactionId);
        if (transaction != null)
        {
            transaction.Status = "Refunded";
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Refund processed successfully for TransactionId: {TransactionId}", context.Message.TransactionId);
    }
}
