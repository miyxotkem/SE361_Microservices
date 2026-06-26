using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Payment.API.IntegrationTests;

public class PaymentApiTests : IClassFixture<PaymentApiFactory>
{
    private readonly PaymentApiFactory _factory;

    public PaymentApiTests(PaymentApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOkOrDegraded()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("health");

        // Should return a response (since it spins up correctly)
        // It might return Degraded or Unhealthy if Redis/RabbitMQ is offline, 
        // but we just want to ensure it has successfully started up.
        response.StatusCode.Should().Match(s => s == HttpStatusCode.OK || s == HttpStatusCode.ServiceUnavailable);
    }
}
