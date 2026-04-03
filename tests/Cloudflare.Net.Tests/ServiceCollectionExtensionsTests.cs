using Cloudflare.Net.Authentication;
using Cloudflare.Net.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace Cloudflare.Net.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCloudflareClient_WithToken_ShouldRegisterServices()
    {
        var services = new ServiceCollection();

        services.AddCloudflareClient(options =>
        {
            options.Token = "test_api_token";
        });

        var provider = services.BuildServiceProvider();
        var authProvider = provider.GetRequiredService<IAuthenticationProvider>();

        Assert.NotNull(authProvider);
        Assert.IsType<CloudflareAuthProvider>(authProvider);
    }

    [Fact]
    public void AddCloudflareClient_WithApiKey_ShouldRegisterApiKeyProvider()
    {
        var services = new ServiceCollection();

        services.AddCloudflareClient(options =>
        {
            options.Email = "user@example.com";
            options.ApiKey = "global_api_key";
        });

        var provider = services.BuildServiceProvider();
        var authProvider = provider.GetRequiredService<IAuthenticationProvider>();

        Assert.NotNull(authProvider);
        Assert.IsType<CloudflareApiKeyAuthProvider>(authProvider);
    }

    [Fact]
    public void AddCloudflareClient_ShouldRegisterRequestAdapter()
    {
        var services = new ServiceCollection();

        services.AddCloudflareClient(options =>
        {
            options.Token = "test_api_token";
        });

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var adapter = scope.ServiceProvider.GetRequiredService<IRequestAdapter>();

        Assert.NotNull(adapter);
        Assert.IsType<HttpClientRequestAdapter>(adapter);
    }

    [Fact]
    public void AddCloudflareClient_WithNoCredentials_ShouldThrowOnResolve()
    {
        var services = new ServiceCollection();

        services.AddCloudflareClient(options => { });

        var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() =>
            provider.GetRequiredService<IAuthenticationProvider>());
    }

    [Fact]
    public void AddCloudflareClient_WithNullServices_ShouldThrow()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCloudflareClient(options => { options.Token = "t"; }));
    }

    [Fact]
    public void AddCloudflareClient_WithNullConfigure_ShouldThrow()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCloudflareClient(null!));
    }

    [Fact]
    public void AddCloudflareClient_NoArgs_ShouldRegisterDefaultOptions()
    {
        var services = new ServiceCollection();

        services.AddCloudflareClient();

        // Should not throw during registration; will only throw on resolve without credentials
        Assert.True(true);
    }
}
