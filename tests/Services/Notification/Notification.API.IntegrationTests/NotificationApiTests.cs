using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Notification.API.IntegrationTests;

public class NotificationApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(FirestoreDb));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var client = new Google.Cloud.Firestore.V1.FirestoreClientBuilder
            {
                Endpoint = "localhost:8888",
                ChannelCredentials = Grpc.Core.ChannelCredentials.Insecure
            }.Build();
            
            var dummyDb = FirestoreDb.Create("notification-db-9b061", client);
            services.AddSingleton(dummyDb);
        });

        builder.UseEnvironment("Development");
    }
}

public class NotificationApiTests : IClassFixture<NotificationApiFactory>
{
    private readonly NotificationApiFactory _factory;

    public NotificationApiTests(NotificationApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOkOrDegraded()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("health");

        response.StatusCode.Should().Match(s => s == HttpStatusCode.OK || s == HttpStatusCode.ServiceUnavailable);
    }
}
