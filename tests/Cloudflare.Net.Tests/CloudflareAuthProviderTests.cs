using Cloudflare.Net.Authentication;
using Microsoft.Kiota.Abstractions;
using Xunit;

namespace Cloudflare.Net.Tests;

public class CloudflareAuthProviderTests
{
    [Fact]
    public void Constructor_WithValidToken_ShouldNotThrow()
    {
        var provider = new CloudflareAuthProvider("test_api_token");
        Assert.NotNull(provider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidToken_ShouldThrow(string? token)
    {
        Assert.Throws<ArgumentException>(() => new CloudflareAuthProvider(token!));
    }

    [Fact]
    public async Task AuthenticateRequestAsync_ShouldAddBearerToken()
    {
        const string token = "test_api_token";
        var provider = new CloudflareAuthProvider(token);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://api.cloudflare.com/client/v4/zones")
        };

        await provider.AuthenticateRequestAsync(request);

        Assert.Contains("Authorization", request.Headers.Keys);
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithNullRequest_ShouldThrow()
    {
        var provider = new CloudflareAuthProvider("test_token");
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.AuthenticateRequestAsync(null!));
    }
}
