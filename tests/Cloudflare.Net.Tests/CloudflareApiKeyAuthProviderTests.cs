using Cloudflare.Net.Authentication;
using Microsoft.Kiota.Abstractions;
using Xunit;

namespace Cloudflare.Net.Tests;

public class CloudflareApiKeyAuthProviderTests
{
    [Fact]
    public void Constructor_WithValidCredentials_ShouldNotThrow()
    {
        var provider = new CloudflareApiKeyAuthProvider("user@example.com", "api_key_123");
        Assert.NotNull(provider);
    }

    [Theory]
    [InlineData(null, "key")]
    [InlineData("", "key")]
    [InlineData("   ", "key")]
    public void Constructor_WithInvalidEmail_ShouldThrow(string? email, string apiKey)
    {
        Assert.Throws<ArgumentException>(() => new CloudflareApiKeyAuthProvider(email!, apiKey));
    }

    [Theory]
    [InlineData("user@example.com", null)]
    [InlineData("user@example.com", "")]
    [InlineData("user@example.com", "   ")]
    public void Constructor_WithInvalidApiKey_ShouldThrow(string email, string? apiKey)
    {
        Assert.Throws<ArgumentException>(() => new CloudflareApiKeyAuthProvider(email, apiKey!));
    }

    [Fact]
    public async Task AuthenticateRequestAsync_ShouldAddEmailAndKeyHeaders()
    {
        const string email = "user@example.com";
        const string apiKey = "api_key_123";
        var provider = new CloudflareApiKeyAuthProvider(email, apiKey);
        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://api.cloudflare.com/client/v4/zones")
        };

        await provider.AuthenticateRequestAsync(request);

        Assert.Contains("X-Auth-Email", request.Headers.Keys);
        Assert.Contains("X-Auth-Key", request.Headers.Keys);
    }

    [Fact]
    public async Task AuthenticateRequestAsync_WithNullRequest_ShouldThrow()
    {
        var provider = new CloudflareApiKeyAuthProvider("user@example.com", "key");
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.AuthenticateRequestAsync(null!));
    }
}
