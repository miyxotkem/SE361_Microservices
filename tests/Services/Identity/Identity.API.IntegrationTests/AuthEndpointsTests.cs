using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Identity.API.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly IdentityApiFactory _factory;

    public AuthEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthWorkflow_ShouldRegisterAndLoginSuccessfully()
    {
        // Create HTTP Client
        var client = _factory.CreateClient();

        // 1. Register a new user
        var registerRequest = new
        {
            Email = "integrationtest@gmail.com",
            Password = "Password123!",
            FullName = "Integration Test User"
        };

        var registerResponse = await client.PostAsJsonAsync("api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Try to register with duplicate email (should fail)
        var duplicateRegisterResponse = await client.PostAsJsonAsync("api/auth/register", registerRequest);
        duplicateRegisterResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 3. Login with the registered user
        var loginRequest = new
        {
            Email = "integrationtest@gmail.com",
            Password = "Password123!"
        };

        var loginResponse = await client.PostAsJsonAsync("api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
        loginResult.Should().NotBeNull();
    }
}
