using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace Cloudflare.Net.Tests;

public class CloudflareClientFactoryTests
{
    [Fact]
    public void Create_WithToken_ShouldReturnAdapter()
    {
        var adapter = CloudflareClientFactory.Create("test_api_token");

        Assert.NotNull(adapter);
        Assert.IsType<HttpClientRequestAdapter>(adapter);
    }

    [Fact]
    public void Create_WithEmailAndApiKey_ShouldReturnAdapter()
    {
        var adapter = CloudflareClientFactory.Create("user@example.com", "global_api_key");

        Assert.NotNull(adapter);
        Assert.IsType<HttpClientRequestAdapter>(adapter);
    }

    [Fact]
    public void Create_WithOptions_Token_ShouldReturnAdapter()
    {
        var options = new CloudflareClientOptions
        {
            Token = "test_api_token",
            Timeout = TimeSpan.FromSeconds(60),
            BaseUrl = "https://api.cloudflare.com/client/v4"
        };

        var adapter = CloudflareClientFactory.Create(options);

        Assert.NotNull(adapter);
        Assert.IsType<HttpClientRequestAdapter>(adapter);
    }

    [Fact]
    public void Create_WithOptions_ApiKey_ShouldReturnAdapter()
    {
        var options = new CloudflareClientOptions
        {
            Email = "user@example.com",
            ApiKey = "global_api_key"
        };

        var adapter = CloudflareClientFactory.Create(options);

        Assert.NotNull(adapter);
        Assert.IsType<HttpClientRequestAdapter>(adapter);
    }

    [Fact]
    public void Create_WithNullOptions_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CloudflareClientFactory.Create((CloudflareClientOptions)null!));
    }

    [Fact]
    public void Create_WithNoCredentials_ShouldThrow()
    {
        var options = new CloudflareClientOptions();

        Assert.Throws<InvalidOperationException>(() =>
            CloudflareClientFactory.Create(options));
    }

    [Fact]
    public void Create_WithHttpClient_ShouldReturnAdapter()
    {
        using var httpClient = new HttpClient();
        var adapter = CloudflareClientFactory.Create("test_api_token", httpClient);

        Assert.NotNull(adapter);
        Assert.IsType<HttpClientRequestAdapter>(adapter);
    }

    [Fact]
    public void Create_WithEmptyToken_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CloudflareClientFactory.Create(""));
    }

    [Fact]
    public void CreateFromEnvironment_WithNoEnvVar_ShouldThrow()
    {
        var original = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("CLOUDFLARE_API_TOKEN", null);

            Assert.Throws<InvalidOperationException>(() =>
                CloudflareClientFactory.CreateFromEnvironment());
        }
        finally
        {
            Environment.SetEnvironmentVariable("CLOUDFLARE_API_TOKEN", original);
        }
    }

    [Fact]
    public void CreateFromEnvironment_WithEnvVar_ShouldReturnAdapter()
    {
        var original = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("CLOUDFLARE_API_TOKEN", "test_token_from_env");

            var adapter = CloudflareClientFactory.CreateFromEnvironment();

            Assert.NotNull(adapter);
            Assert.IsType<HttpClientRequestAdapter>(adapter);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CLOUDFLARE_API_TOKEN", original);
        }
    }

    [Fact]
    public void CreateFromWranglerConfigFile_WithValidToml_ShouldReturnAdapter()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "oauth_token = \"test_token_from_wrangler\"\n");

            var adapter = CloudflareClientFactory.CreateFromWranglerConfigFile(tempFile);

            Assert.NotNull(adapter);
            Assert.IsType<HttpClientRequestAdapter>(adapter);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CreateFromWranglerConfigFile_WithApiToken_ShouldReturnAdapter()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "api_token = \"test_api_token_from_wrangler\"\n");

            var adapter = CloudflareClientFactory.CreateFromWranglerConfigFile(tempFile);

            Assert.NotNull(adapter);
            Assert.IsType<HttpClientRequestAdapter>(adapter);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CreateFromWranglerConfigFile_WithConfigureOptions_ShouldApplyOptions()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "oauth_token = \"test_token\"\n");

            var adapter = CloudflareClientFactory.CreateFromWranglerConfigFile(tempFile, options =>
            {
                options.Timeout = TimeSpan.FromMinutes(5);
            });

            Assert.NotNull(adapter);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CreateFromWranglerConfigFile_WithMissingFile_ShouldThrow()
    {
        var ex = Assert.Throws<FileNotFoundException>(() =>
            CloudflareClientFactory.CreateFromWranglerConfigFile(@"C:\nonexistent\config.toml"));

        Assert.Contains("Wrangler config file not found", ex.Message);
    }

    [Fact]
    public void CreateFromWranglerConfigFile_WithEmptyPath_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CloudflareClientFactory.CreateFromWranglerConfigFile(""));
    }

    [Fact]
    public void CreateFromWranglerConfigFile_WithMissingToken_ShouldThrow()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "some_other_key = \"value\"\n");

            var ex = Assert.Throws<InvalidOperationException>(() =>
                CloudflareClientFactory.CreateFromWranglerConfigFile(tempFile));

            Assert.Contains("oauth_token", ex.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
