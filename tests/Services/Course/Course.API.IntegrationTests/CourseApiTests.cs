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

namespace Course.API.IntegrationTests;

public class CourseApiFactory : WebApplicationFactory<Program>
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
                ChannelCredentials = global::Grpc.Core.ChannelCredentials.Insecure
            }.Build();
            
            var dummyDb = FirestoreDb.Create("course-db-28f2a", client);
            services.AddSingleton(dummyDb);
        });

        builder.UseEnvironment("Development");
    }
}

public class CourseApiTests : IClassFixture<CourseApiFactory>
{
    private readonly CourseApiFactory _factory;

    public CourseApiTests(CourseApiFactory factory)
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
