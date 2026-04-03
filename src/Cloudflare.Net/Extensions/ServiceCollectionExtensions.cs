using Cloudflare.Net.Authentication;
using Cloudflare.Net.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Cloudflare.Net.Extensions;

/// <summary>
/// Extension methods for registering Cloudflare API client services with the dependency injection container.
/// 用于在依赖注入容器中注册 Cloudflare API 客户端服务的扩展方法。
/// </summary>
/// <example>
/// <code>
/// // In your Program.cs or Startup.cs / 在 Program.cs 或 Startup.cs 中
/// services.AddCloudflareClient(options =>
/// {
///     options.Token = configuration["Cloudflare:Token"]!;
/// });
///
/// // Then inject the client / 然后注入客户端
/// public class MyService(CloudflareApiClient client)
/// {
///     public async Task DoWork()
///     {
///         var zones = await client.Zones.GetAsync();
///     }
/// }
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Cloudflare API client to the service collection.
    /// Token must be configured separately via <c>IConfiguration</c> binding or other means.
    /// <para>
    /// 将 Cloudflare API 客户端添加到服务集合中。
    /// 令牌需通过 <c>IConfiguration</c> 绑定或其他方式单独配置。
    /// </para>
    /// </summary>
    /// <param name="services">The service collection. / 服务集合。</param>
    /// <returns>The service collection for chaining. / 用于链式调用的服务集合。</returns>
    public static IServiceCollection AddCloudflareClient(
        this IServiceCollection services)
    {
        return AddCloudflareClient(services, _ => { });
    }

    /// <summary>
    /// Adds the Cloudflare API client to the service collection.
    /// 将 Cloudflare API 客户端添加到服务集合中。
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

        services.TryAddScoped(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudflareClientOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("Cloudflare");
            var authProvider = sp.GetRequiredService<IAuthenticationProvider>();

            var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
            {
                BaseUrl = options.BaseUrl
            };

            return new CloudflareApiClient(adapter);
        });

        return services;
    }
}
