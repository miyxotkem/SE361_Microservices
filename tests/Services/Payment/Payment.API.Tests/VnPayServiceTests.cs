using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Xunit;
using Payment.API.Services;

namespace Payment.API.Tests;

public class VnPayServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly VnPayService _vnPayService;

    public VnPayServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "VnPay:TmnCode", "TMN12345" },
            { "VnPay:HashSecret", "SECRET12345" },
            { "VnPay:BaseUrl", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html" },
            { "VnPay:ReturnUrl", "https://localhost:7001/vnpay-return" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _vnPayService = new VnPayService(_configuration);
    }

    [Fact]
    public async Task GeneratePaymentUrlAsync_ShouldReturnCorrectUrlFormat()
    {
        // Act
        var url = await _vnPayService.GeneratePaymentUrlAsync("tx-123", 50000, "course-1", "user-1");

        // Assert
        url.Should().StartWith("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        url.Should().Contain("vnp_TmnCode=TMN12345");
        url.Should().Contain("vnp_Amount=5000000"); // 50000 * 100
        url.Should().Contain("vnp_CurrCode=VND");
        url.Should().Contain("vnp_SecureHash=");
    }

    [Fact]
    public void ValidateSignature_ShouldReturnFalse_WhenHashIsMissing()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "vnp_Amount", "5000000" },
            { "vnp_TxnRef", "tx-123" }
        };

        // Act
        var isValid = _vnPayService.ValidateSignature(parameters);

        // Assert
        isValid.Should().BeFalse();
    }
}
