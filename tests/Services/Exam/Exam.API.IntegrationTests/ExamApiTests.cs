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

namespace Exam.API.IntegrationTests;

public class ExamApiFactory : WebApplicationFactory<Program>
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
            
            var dummyDb = FirestoreDb.Create("exam-db-8e1b4", client);
            services.AddSingleton(dummyDb);
        });

        builder.UseEnvironment("Development");
    }
}

public class ExamApiTests : IClassFixture<ExamApiFactory>
{
    private readonly ExamApiFactory _factory;

    public ExamApiTests(ExamApiFactory factory)
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
