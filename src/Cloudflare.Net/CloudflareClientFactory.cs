using Cloudflare.Net.Authentication;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Tomlyn;
using Tomlyn.Model;

namespace Cloudflare.Net;

/// <summary>
/// Factory class for creating configured Cloudflare API request adapters.
/// Each Cloudflare.Net.* API package provides its own client class that accepts the adapter.
/// <para>
/// 用于创建已配置的 Cloudflare API 请求适配器的工厂类。
/// 每个 Cloudflare.Net.* API 包提供自己的客户端类来接收适配器。
/// </para>
/// </summary>
/// <example>
/// <code>
/// // Create adapter with API Token (recommended) / 使用 API Token 创建适配器（推荐）
/// var adapter = CloudflareClientFactory.Create("your_api_token");
///
/// // Use with any API package client / 与任何 API 包客户端一起使用
/// var zonesClient = new CloudflareZonesClient(adapter);
/// var zones = await zonesClient.Zones.GetAsync();
///
/// // From environment variable / 从环境变量
/// var adapter = CloudflareClientFactory.CreateFromEnvironment();
///
/// // From wrangler config / 从 wrangler 配置
/// var adapter = CloudflareClientFactory.CreateFromWranglerConfig();
/// </code>
/// </example>
public static class CloudflareClientFactory
{
    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> with the specified API token (Bearer authentication).
    /// 使用指定的 API 令牌创建一个新的 <see cref="HttpClientRequestAdapter"/>（Bearer 认证）。
    /// </summary>
    /// <param name="token">The Cloudflare API token. / Cloudflare API 令牌。</param>
    /// <returns>A configured <see cref="HttpClientRequestAdapter"/> instance. / 已配置的 <see cref="HttpClientRequestAdapter"/> 实例。</returns>
    public static HttpClientRequestAdapter Create(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty. / 令牌不能为 null 或空。", nameof(token));
        }

        return Create(new CloudflareClientOptions { Token = token });
    }

    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> with Global API Key authentication.
    /// 使用 Global API Key 认证创建一个新的 <see cref="HttpClientRequestAdapter"/>。
    /// </summary>
    /// <param name="email">The Cloudflare account email. / Cloudflare 账户邮箱。</param>
    /// <param name="apiKey">The Cloudflare Global API Key. / Cloudflare Global API Key。</param>
    /// <returns>A configured <see cref="HttpClientRequestAdapter"/> instance. / 已配置的 <see cref="HttpClientRequestAdapter"/> 实例。</returns>
    public static HttpClientRequestAdapter Create(string email, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty. / 邮箱不能为 null 或空。", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty. / API Key 不能为 null 或空。", nameof(apiKey));
        }

        return Create(new CloudflareClientOptions { Email = email, ApiKey = apiKey });
    }

    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> with the specified options.
    /// 使用指定的选项创建一个新的 <see cref="HttpClientRequestAdapter"/>。
    /// </summary>
    /// <param name="options">The client configuration options. / 客户端配置选项。</param>
    /// <returns>A configured <see cref="HttpClientRequestAdapter"/> instance. / 已配置的 <see cref="HttpClientRequestAdapter"/> 实例。</returns>
    public static HttpClientRequestAdapter Create(CloudflareClientOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var authProvider = CreateAuthProvider(options);
        var httpClient = CreateHttpClient(options);

        return new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
        {
            BaseUrl = options.BaseUrl
        };
    }

    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> with a pre-configured <see cref="HttpClient"/>.
    /// <para>
    /// 使用预配置的 <see cref="HttpClient"/> 创建新的 <see cref="HttpClientRequestAdapter"/>。
    /// </para>
    /// </summary>
    /// <param name="token">The Cloudflare API token. / Cloudflare API 令牌。</param>
    /// <param name="httpClient">The pre-configured HttpClient. / 预配置的 HttpClient。</param>
    /// <param name="baseUrl">The base URL. Defaults to <c>https://api.cloudflare.com/client/v4</c>. / 基础 URL。</param>
    /// <returns>A configured <see cref="HttpClientRequestAdapter"/> instance. / 已配置的 <see cref="HttpClientRequestAdapter"/> 实例。</returns>
    public static HttpClientRequestAdapter Create(string token, HttpClient httpClient, string baseUrl = "https://api.cloudflare.com/client/v4")
    {
        var authProvider = new CloudflareAuthProvider(token);

        return new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
        {
            BaseUrl = baseUrl
        };
    }

    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> by reading the API token from the
    /// <c>CLOUDFLARE_API_TOKEN</c> environment variable.
    /// <para>
    /// 从 <c>CLOUDFLARE_API_TOKEN</c> 环境变量读取 API 令牌来创建新的 <see cref="HttpClientRequestAdapter"/>。
    /// </para>
    /// </summary>
    public static HttpClientRequestAdapter CreateFromEnvironment(Action<CloudflareClientOptions>? configureOptions = null)
    {
        var token = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Environment variable 'CLOUDFLARE_API_TOKEN' is not set or is empty. / " +
                "环境变量 'CLOUDFLARE_API_TOKEN' 未设置或为空。");
        }

        var options = new CloudflareClientOptions { Token = token };
        configureOptions?.Invoke(options);

        return Create(options);
    }

    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> by reading the API token from the
    /// default wrangler configuration file at <c>~/.wrangler/config/default.toml</c>.
    /// <para>
    /// 从默认 wrangler 配置文件读取 API 令牌来创建新的 <see cref="HttpClientRequestAdapter"/>。
    /// </para>
    /// </summary>
    public static HttpClientRequestAdapter CreateFromWranglerConfig(Action<CloudflareClientOptions>? configureOptions = null)
    {
        var configPath = GetWranglerConfigPath();
        return CreateFromWranglerConfigFile(configPath, configureOptions);
    }

    /// <summary>
    /// Creates a new <see cref="HttpClientRequestAdapter"/> by reading the API token from a specified
    /// wrangler TOML configuration file.
    /// <para>
    /// 从指定的 wrangler TOML 配置文件读取 API 令牌来创建新的 <see cref="HttpClientRequestAdapter"/>。
    /// </para>
    /// </summary>
    public static HttpClientRequestAdapter CreateFromWranglerConfigFile(string configFilePath, Action<CloudflareClientOptions>? configureOptions = null)
    {
        if (string.IsNullOrWhiteSpace(configFilePath))
        {
            throw new ArgumentException(
                "Config file path cannot be null or empty. / 配置文件路径不能为 null 或空。",
                nameof(configFilePath));
        }

        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException(
                $"Wrangler config file not found at: {configFilePath}. / " +
                $"Wrangler 配置文件未找到：{configFilePath}。",
                configFilePath);
        }

        var token = ReadTokenFromWranglerConfig(configFilePath);

        var options = new CloudflareClientOptions { Token = token };
        configureOptions?.Invoke(options);

        return Create(options);
    }

    internal static IAuthenticationProvider CreateAuthProvider(CloudflareClientOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            return new CloudflareAuthProvider(options.Token);
        }

        if (!string.IsNullOrWhiteSpace(options.Email) && !string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return new CloudflareApiKeyAuthProvider(options.Email!, options.ApiKey!);
        }

        throw new InvalidOperationException(
            "No valid authentication credentials provided. " +
            "Please set either 'Token' (recommended) or both 'Email' and 'ApiKey'. / " +
            "未提供有效的认证凭据。请设置 'Token'（推荐）或同时设置 'Email' 和 'ApiKey'。");
    }

    private static string GetWranglerConfigPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".wrangler", "config", "default.toml");
    }

    private static string ReadTokenFromWranglerConfig(string configFilePath)
    {
        var toml = File.ReadAllText(configFilePath);
        var model = Toml.ToModel(toml);

        if (model.TryGetValue("oauth_token", out var oauthToken) && oauthToken is string oauthStr && !string.IsNullOrWhiteSpace(oauthStr))
        {
            return oauthStr;
        }

        if (model.TryGetValue("api_token", out var apiToken) && apiToken is string apiStr && !string.IsNullOrWhiteSpace(apiStr))
        {
            return apiStr;
        }

        throw new InvalidOperationException(
            $"No 'oauth_token' or 'api_token' found in wrangler config file: {configFilePath}. / " +
            $"在 wrangler 配置文件中未找到 'oauth_token' 或 'api_token'：{configFilePath}。");
    }

    internal static HttpClient CreateHttpClient(CloudflareClientOptions options)
    {
        var httpClient = new HttpClient
        {
            Timeout = options.Timeout,
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);

        return httpClient;
    }
}
