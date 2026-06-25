using System;

namespace Course.API.Features.Registrations.Sagas
{
    public class PaymentTimeoutExpired
    {
        public Guid CorrelationId { get; set; }
    }
}
