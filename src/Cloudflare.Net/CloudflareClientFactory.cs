using Cloudflare.Net.Authentication;
using Cloudflare.Net.Generated;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Tomlyn;
using Tomlyn.Model;

namespace Cloudflare.Net;

/// <summary>
/// Factory class for creating Cloudflare API clients.
/// 用于创建 Cloudflare API 客户端的工厂类。
/// </summary>
/// <example>
/// <code>
/// // API Token (recommended) / API Token（推荐）
/// var client = CloudflareClientFactory.Create("your_api_token");
///
/// // Global API Key / Global API Key
/// var client = CloudflareClientFactory.Create("email@example.com", "your_global_api_key");
///
/// // From environment variable / 从环境变量
/// var client = CloudflareClientFactory.CreateFromEnvironment();
///
/// // From wrangler config / 从 wrangler 配置
/// var client = CloudflareClientFactory.CreateFromWranglerConfig();
///
/// // With options / 使用选项
/// var client = CloudflareClientFactory.Create(new CloudflareClientOptions
/// {
///     Token = "your_api_token",
///     Timeout = TimeSpan.FromSeconds(60)
/// });
/// </code>
/// </example>
public static class CloudflareClientFactory
{
    /// <summary>
    /// Creates a new <see cref="CloudflareApiClient"/> with the specified API token (Bearer authentication).
    /// 使用指定的 API 令牌创建一个新的 <see cref="CloudflareApiClient"/>（Bearer 认证）。
    /// </summary>
    /// <param name="token">The Cloudflare API token. / Cloudflare API 令牌。</param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    public static CloudflareApiClient Create(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty. / 令牌不能为 null 或空。", nameof(token));
        }

        return Create(new CloudflareClientOptions { Token = token });
    }

    /// <summary>
    /// Creates a new <see cref="CloudflareApiClient"/> with Global API Key authentication.
    /// 使用 Global API Key 认证创建一个新的 <see cref="CloudflareApiClient"/>。
    /// </summary>
    /// <param name="email">The Cloudflare account email. / Cloudflare 账户邮箱。</param>
    /// <param name="apiKey">The Cloudflare Global API Key. / Cloudflare Global API Key。</param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    public static CloudflareApiClient Create(string email, string apiKey)
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
    /// Creates a new <see cref="CloudflareApiClient"/> with the specified options.
    /// 使用指定的选项创建一个新的 <see cref="CloudflareApiClient"/>。
    /// </summary>
    /// <param name="options">The client configuration options. / 客户端配置选项。</param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null. / 当选项为 null 时抛出。</exception>
    /// <exception cref="InvalidOperationException">Thrown when no valid authentication credentials are provided. / 当未提供有效认证凭据时抛出。</exception>
    public static CloudflareApiClient Create(CloudflareClientOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var authProvider = CreateAuthProvider(options);
        var httpClient = CreateHttpClient(options);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
        {
            BaseUrl = options.BaseUrl
        };

        return new CloudflareApiClient(adapter);
    }

    /// <summary>
    /// Creates a new <see cref="CloudflareApiClient"/> with a pre-configured <see cref="HttpClient"/>.
    /// Useful for advanced scenarios such as custom message handlers or proxies.
    /// <para>
    /// 使用预配置的 <see cref="HttpClient"/> 创建新的 <see cref="CloudflareApiClient"/>。
    /// 适用于自定义消息处理程序或代理等高级场景。
    /// </para>
    /// </summary>
    /// <param name="token">The Cloudflare API token. / Cloudflare API 令牌。</param>
    /// <param name="httpClient">The pre-configured HttpClient. / 预配置的 HttpClient。</param>
    /// <param name="baseUrl">The base URL. Defaults to <c>https://api.cloudflare.com/client/v4</c>. / 基础 URL。</param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    public static CloudflareApiClient Create(string token, HttpClient httpClient, string baseUrl = "https://api.cloudflare.com/client/v4")
    {
        var authProvider = new CloudflareAuthProvider(token);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
        {
            BaseUrl = baseUrl
        };

        return new CloudflareApiClient(adapter);
    }

    /// <summary>
    /// Creates a new <see cref="CloudflareApiClient"/> by reading the API token from the
    /// <c>CLOUDFLARE_API_TOKEN</c> environment variable.
    /// <para>
    /// 从 <c>CLOUDFLARE_API_TOKEN</c> 环境变量读取 API 令牌来创建新的 <see cref="CloudflareApiClient"/>。
    /// </para>
    /// </summary>
    /// <param name="configureOptions">
    /// Optional action to further configure client options (e.g., timeout, base URL).
    /// The token will be pre-populated from the environment variable.
    /// <para>可选的配置操作，用于进一步配置客户端选项。令牌将从环境变量中预填充。</para>
    /// </param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    /// <exception cref="InvalidOperationException">Thrown when the environment variable is not set. / 当环境变量未设置时抛出。</exception>
    public static CloudflareApiClient CreateFromEnvironment(Action<CloudflareClientOptions>? configureOptions = null)
    {
        var token = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Environment variable 'CLOUDFLARE_API_TOKEN' is not set or is empty. " +
                "Please set it to your Cloudflare API token. / " +
                "环境变量 'CLOUDFLARE_API_TOKEN' 未设置或为空。请将其设置为您的 Cloudflare API 令牌。");
        }

        var options = new CloudflareClientOptions { Token = token };
        configureOptions?.Invoke(options);

        return Create(options);
    }

    /// <summary>
    /// Creates a new <see cref="CloudflareApiClient"/> by reading the API token from the
    /// default wrangler configuration file at <c>~/.wrangler/config/default.toml</c>.
    /// <para>
    /// 从默认 wrangler 配置文件 <c>~/.wrangler/config/default.toml</c> 读取 API 令牌来创建新的 <see cref="CloudflareApiClient"/>。
    /// </para>
    /// </summary>
    /// <param name="configureOptions">
    /// Optional action to further configure client options.
    /// <para>可选的配置操作，用于进一步配置客户端选项。</para>
    /// </param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    /// <exception cref="FileNotFoundException">Thrown when the wrangler config file is not found. / 当 wrangler 配置文件不存在时抛出。</exception>
    /// <exception cref="InvalidOperationException">Thrown when the API token is not found in the config file. / 当配置文件中未找到 API 令牌时抛出。</exception>
    public static CloudflareApiClient CreateFromWranglerConfig(Action<CloudflareClientOptions>? configureOptions = null)
    {
        var configPath = GetWranglerConfigPath();
        return CreateFromWranglerConfigFile(configPath, configureOptions);
    }

    /// <summary>
    /// Creates a new <see cref="CloudflareApiClient"/> by reading the API token from a specified
    /// wrangler TOML configuration file.
    /// <para>
    /// 从指定的 wrangler TOML 配置文件读取 API 令牌来创建新的 <see cref="CloudflareApiClient"/>。
    /// </para>
    /// </summary>
    /// <param name="configFilePath">The full path to the wrangler config TOML file. / wrangler 配置 TOML 文件的完整路径。</param>
    /// <param name="configureOptions">
    /// Optional action to further configure client options.
    /// <para>可选的配置操作，用于进一步配置客户端选项。</para>
    /// </param>
    /// <returns>A configured <see cref="CloudflareApiClient"/> instance. / 已配置的 <see cref="CloudflareApiClient"/> 实例。</returns>
    /// <exception cref="ArgumentException">Thrown when configFilePath is null or empty. / 当配置文件路径为 null 或空时抛出。</exception>
    /// <exception cref="FileNotFoundException">Thrown when the config file is not found. / 当配置文件不存在时抛出。</exception>
    /// <exception cref="InvalidOperationException">Thrown when the API token is not found in the config file. / 当配置文件中未找到 API 令牌时抛出。</exception>
    public static CloudflareApiClient CreateFromWranglerConfigFile(string configFilePath, Action<CloudflareClientOptions>? configureOptions = null)
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
                $"Wrangler config file not found at: {configFilePath}. " +
                $"Please install wrangler and run 'wrangler login' first. / " +
                $"Wrangler 配置文件未找到：{configFilePath}。请先安装 wrangler 并运行 'wrangler login'。",
                configFilePath);
        }

        var token = ReadTokenFromWranglerConfig(configFilePath);

        var options = new CloudflareClientOptions { Token = token };
        configureOptions?.Invoke(options);

        return Create(options);
    }

    private static IAuthenticationProvider CreateAuthProvider(CloudflareClientOptions options)
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
            "No valid authentication credentials provided. "+
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

        // Try "oauth_token" first (wrangler login result), then "api_token"
        if (model.TryGetValue("oauth_token", out var oauthToken) && oauthToken is string oauthStr && !string.IsNullOrWhiteSpace(oauthStr))
        {
            return oauthStr;
        }

        if (model.TryGetValue("api_token", out var apiToken) && apiToken is string apiStr && !string.IsNullOrWhiteSpace(apiStr))
        {
            return apiStr;
        }

        throw new InvalidOperationException(
            $"No 'oauth_token' or 'api_token' found in wrangler config file: {configFilePath}. " +
            $"Please run 'wrangler login' to configure your token. / " +
            $"在 wrangler 配置文件中未找到 'oauth_token' 或 'api_token'：{configFilePath}。请运行 'wrangler login' 配置令牌。");
    }

    private static HttpClient CreateHttpClient(CloudflareClientOptions options)
    {
        var httpClient = new HttpClient
        {
            Timeout = options.Timeout,
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);

        return httpClient;
    }
}
