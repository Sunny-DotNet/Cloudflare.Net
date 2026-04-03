using Cloudflare.Net.Authentication;
using Microsoft.Kiota.Abstractions;
using Xunit;

namespace Cloudflare.Net.Tests;

public class CloudflareOriginCaAuthProviderTests
{
    [Fact]
    public void Constructor_WithValidKey_ShouldNotThrow()
    {
        var provider = new CloudflareOriginCaAuthProvider("v1.0-service_key_123");
        Assert.NotNull(provider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidKey_ShouldThrow(string? serviceKey)
    {
        Assert.Throws<ArgumentException>(() => new CloudflareOriginCaAuthProvider(serviceKey!));
    }

    [Fact]
    public async Task AuthenticateRequestAsync_ShouldAddServiceKeyHeader()
    {
        const string serviceKey = "v1.0-service_key_123";
        var provider = new CloudflareOriginCaAuthProvider(serviceKey);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://api.cloudflare.com/client/v4/certificates")
        };

        await provider.AuthenticateRequestAsync(request);

        Assert.Contains("X-Auth-User-Service-Key", request.Headers.Keys);
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithNullRequest_ShouldThrow()
    {
        var provider = new CloudflareOriginCaAuthProvider("v1.0-key");
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.AuthenticateRequestAsync(null!));
    }
}
