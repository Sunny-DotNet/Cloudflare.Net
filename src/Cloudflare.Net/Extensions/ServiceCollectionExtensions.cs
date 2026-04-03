using Cloudflare.Net.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Cloudflare.Net.Extensions;

/// <summary>
/// Extension methods for registering Cloudflare API services with the dependency injection container.
/// Each Cloudflare.Net.* API package provides additional extension methods to register its specific client.
/// <para>
/// 用于在依赖注入容器中注册 Cloudflare API 服务的扩展方法。
/// 每个 Cloudflare.Net.* API 包提供额外的扩展方法来注册其特定的客户端。
/// </para>
/// </summary>
/// <example>
/// <code>
/// // Register core services / 注册核心服务
/// services.AddCloudflareClient(options =>
/// {
///     options.Token = configuration["Cloudflare:Token"]!;
/// });
///
/// // Then register API-specific clients from each package / 然后从各包注册 API 客户端
/// // services.AddCloudflareZones();
/// // services.AddCloudflareWorkers();
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core Cloudflare API services (authentication, HTTP client, request adapter) to the service collection.
    /// <para>将核心 Cloudflare API 服务（认证、HTTP 客户端、请求适配器）添加到服务集合中。</para>
    /// </summary>
    /// <param name="services">The service collection. / 服务集合。</param>
    /// <returns>The service collection for chaining. / 用于链式调用的服务集合。</returns>
    public static IServiceCollection AddCloudflareClient(
        this IServiceCollection services)
    {
        return AddCloudflareClient(services, _ => { });
    }

    /// <summary>
    /// Adds core Cloudflare API services to the service collection with the specified options.
    /// <para>使用指定的选项将核心 Cloudflare API 服务添加到服务集合中。</para>
    /// </summary>
    /// <param name="services">The service collection. / 服务集合。</param>
    /// <param name="configureOptions">An action to configure the client options. / 配置客户端选项的操作。</param>
    /// <returns>The service collection for chaining. / 用于链式调用的服务集合。</returns>
    public static IServiceCollection AddCloudflareClient(
        this IServiceCollection services,
        Action<CloudflareClientOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        services.Configure(configureOptions);

        services.TryAddSingleton<IAuthenticationProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudflareClientOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(options.Token))
            {
                return new CloudflareAuthProvider(options.Token);
            }

            if (!string.IsNullOrWhiteSpace(options.Email) && !string.IsNullOrWhiteSpace(options.ApiKey))
            {
                return new CloudflareApiKeyAuthProvider(options.Email!, options.ApiKey!);
            }

            throw new InvalidOperationException(
                "No valid authentication credentials configured. "+
                "Please set either 'Token' or both 'Email' and 'ApiKey' in CloudflareClientOptions. / " +
                "未配置有效的认证凭据。请在 CloudflareClientOptions 中设置 'Token' 或同时设置 'Email' 和 'ApiKey'。");
        });

        services.AddHttpClient("Cloudflare", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CloudflareClientOptions>>().Value;
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        });

        services.TryAddScoped<IRequestAdapter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudflareClientOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("Cloudflare");
            var authProvider = sp.GetRequiredService<IAuthenticationProvider>();

            return new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
            {
                BaseUrl = options.BaseUrl
            };
        });

        return services;
    }
}
