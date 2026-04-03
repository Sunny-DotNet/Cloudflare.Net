using Xunit;

namespace Cloudflare.Net.Tests;

public class CloudflareClientOptionsTests
{
    [Fact]
    public void DefaultOptions_ShouldHaveCorrectDefaults()
    {
        var options = new CloudflareClientOptions();

        Assert.Equal(string.Empty, options.Token);
        Assert.Null(options.Email);
        Assert.Null(options.ApiKey);
        Assert.Equal("https://api.cloudflare.com/client/v4", options.BaseUrl);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.StartsWith("Cloudflare.Net/", options.UserAgent);
    }

    [Fact]
    public void Options_ShouldAllowTokenCustomization()
    {
        var options = new CloudflareClientOptions
        {
            Token = "my_token",
            BaseUrl = "https://custom.api.com",
            Timeout = TimeSpan.FromMinutes(2),
            UserAgent = "MyApp/1.0"
        };

        Assert.Equal("my_token", options.Token);
        Assert.Equal("https://custom.api.com", options.BaseUrl);
        Assert.Equal(TimeSpan.FromMinutes(2), options.Timeout);
        Assert.Equal("MyApp/1.0", options.UserAgent);
    }

    [Fact]
    public void Options_ShouldAllowApiKeyCustomization()
    {
        var options = new CloudflareClientOptions
        {
            Email = "user@example.com",
            ApiKey = "global_api_key"
        };

        Assert.Equal("user@example.com", options.Email);
        Assert.Equal("global_api_key", options.ApiKey);
    }
}
